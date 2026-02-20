using MediatR;
using Microsoft.EntityFrameworkCore;
using Seems.Application.Common.Interfaces;
using Seems.Application.Media.Dtos;

namespace Seems.Application.Media.Commands.RenameMediaFolder;

public class RenameMediaFolderHandler(
    IAppDbContext db,
    ICurrentUser currentUser) : IRequestHandler<RenameMediaFolderCommand, MediaFolderDto>
{
    public async Task<MediaFolderDto> Handle(RenameMediaFolderCommand request, CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId
            ?? throw new UnauthorizedAccessException("User not authenticated.");

        var folder = await db.MediaFolders
            .Include(f => f.Children)
            .Include(f => f.Items)
            .FirstOrDefaultAsync(f => f.Id == request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Folder {request.Id} not found.");

        if (folder.OwnerId != userId && !currentUser.Roles.Contains("Admin"))
            throw new UnauthorizedAccessException("You do not have access to this folder.");

        var duplicate = await db.MediaFolders.AnyAsync(
            f => f.Id != request.Id &&
                 f.OwnerId == folder.OwnerId &&
                 f.ParentId == folder.ParentId &&
                 f.Name == request.Name,
            cancellationToken);

        if (duplicate)
            throw new InvalidOperationException($"A folder named '{request.Name}' already exists here.");

        folder.Name = request.Name;
        folder.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(cancellationToken);

        return new MediaFolderDto
        {
            Id = folder.Id,
            Name = folder.Name,
            OwnerId = folder.OwnerId,
            ParentId = folder.ParentId,
            ChildCount = folder.Children.Count,
            MediaCount = folder.Items.Count,
            CreatedAt = folder.CreatedAt,
        };
    }
}
