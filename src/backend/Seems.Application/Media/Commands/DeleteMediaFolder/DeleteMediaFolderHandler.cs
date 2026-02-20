using MediatR;
using Microsoft.EntityFrameworkCore;
using Seems.Application.Common.Interfaces;
using Seems.Domain.Interfaces;

namespace Seems.Application.Media.Commands.DeleteMediaFolder;

public class DeleteMediaFolderHandler(
    IAppDbContext db,
    IStorageProviderFactory storageFactory,
    ICurrentUser currentUser) : IRequestHandler<DeleteMediaFolderCommand>
{
    public async Task Handle(DeleteMediaFolderCommand request, CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId
            ?? throw new UnauthorizedAccessException("User not authenticated.");

        var folder = await db.MediaFolders.FindAsync([request.Id], cancellationToken)
            ?? throw new KeyNotFoundException($"Folder {request.Id} not found.");

        if (folder.OwnerId != userId && !currentUser.Roles.Contains("Admin"))
            throw new UnauthorizedAccessException("You do not have access to this folder.");

        // Collect all media items within this folder subtree before cascade delete removes them
        var allMediaKeys = await CollectStorageKeysAsync(request.Id, cancellationToken);

        // Remove folder (cascade deletes children + sets media.FolderId = null via DB)
        db.MediaFolders.Remove(folder);
        await db.SaveChangesAsync(cancellationToken);

        // Clean up storage files for all affected media
        var provider = await storageFactory.GetCurrentAsync(cancellationToken);
        foreach (var key in allMediaKeys)
        {
            try { await provider.DeleteAsync(key, cancellationToken); }
            catch { /* best-effort storage cleanup */ }
        }
    }

    private async Task<List<string>> CollectStorageKeysAsync(Guid folderId, CancellationToken ct)
    {
        var keys = new List<string>();
        var queue = new Queue<Guid>();
        queue.Enqueue(folderId);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();

            var mediaKeys = await db.Media
                .Where(m => m.FolderId == current)
                .Select(m => m.StorageKey)
                .ToListAsync(ct);
            keys.AddRange(mediaKeys);

            var childIds = await db.MediaFolders
                .Where(f => f.ParentId == current)
                .Select(f => f.Id)
                .ToListAsync(ct);
            foreach (var id in childIds) queue.Enqueue(id);
        }

        return keys;
    }
}
