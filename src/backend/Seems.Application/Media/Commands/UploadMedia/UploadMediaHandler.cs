using MediatR;
using Microsoft.EntityFrameworkCore;
using Seems.Application.Common.Interfaces;
using Seems.Application.Media.Dtos;
using Seems.Domain.Interfaces;

namespace Seems.Application.Media.Commands.UploadMedia;

public class UploadMediaHandler(
    IAppDbContext db,
    IStorageProviderFactory storageFactory,
    ICurrentUser currentUser) : IRequestHandler<UploadMediaCommand, MediaDto>
{
    public async Task<MediaDto> Handle(UploadMediaCommand request, CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId
            ?? throw new UnauthorizedAccessException("User not authenticated.");

        if (request.FolderId.HasValue)
        {
            var folder = await db.MediaFolders.FindAsync([request.FolderId.Value], cancellationToken)
                ?? throw new KeyNotFoundException($"Folder {request.FolderId} not found.");

            if (folder.OwnerId != userId && !currentUser.Roles.Contains("Admin"))
                throw new UnauthorizedAccessException("You do not have access to this folder.");
        }

        var provider = await storageFactory.GetCurrentAsync(cancellationToken);
        var result = await provider.UploadAsync(request.Content, request.FileName, request.ContentType, userId, cancellationToken);

        var media = new Domain.Entities.Media
        {
            Id = Guid.NewGuid(),
            FileName = result.StorageKey.Split('/').Last(),
            OriginalName = request.FileName,
            StorageKey = result.StorageKey,
            Url = result.PublicUrl,
            MimeType = request.ContentType,
            Size = request.Size,
            OwnerId = userId,
            FolderId = request.FolderId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };

        db.Media.Add(media);
        await db.SaveChangesAsync(cancellationToken);

        var ownerEmail = await db.Users
            .Where(u => u.Id == userId)
            .Select(u => u.Email)
            .FirstOrDefaultAsync(cancellationToken);

        return ToDto(media, ownerEmail);
    }

    private static MediaDto ToDto(Domain.Entities.Media m, string? ownerEmail) => new()
    {
        Id = m.Id,
        FileName = m.FileName,
        OriginalName = m.OriginalName,
        Url = m.Url,
        MimeType = m.MimeType,
        Size = m.Size,
        AltText = m.AltText,
        Caption = m.Caption,
        OwnerId = m.OwnerId,
        OwnerEmail = ownerEmail,
        FolderId = m.FolderId,
        CreatedAt = m.CreatedAt,
    };
}
