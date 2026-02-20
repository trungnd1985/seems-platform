using MediatR;
using Microsoft.EntityFrameworkCore;
using Seems.Application.Common.Interfaces;
using Seems.Application.Media.Dtos;

namespace Seems.Application.Media.Commands.MoveMedia;

public class MoveMediaHandler(
    IAppDbContext db,
    ICurrentUser currentUser) : IRequestHandler<MoveMediaCommand, MediaDto>
{
    public async Task<MediaDto> Handle(MoveMediaCommand request, CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId
            ?? throw new UnauthorizedAccessException("User not authenticated.");

        var media = await db.Media.FindAsync([request.Id], cancellationToken)
            ?? throw new KeyNotFoundException($"Media {request.Id} not found.");

        if (media.OwnerId != userId && !currentUser.Roles.Contains("Admin"))
            throw new UnauthorizedAccessException("You do not have access to this file.");

        if (request.TargetFolderId.HasValue)
        {
            var folder = await db.MediaFolders.FindAsync([request.TargetFolderId.Value], cancellationToken)
                ?? throw new KeyNotFoundException($"Folder {request.TargetFolderId} not found.");

            if (folder.OwnerId != userId && !currentUser.Roles.Contains("Admin"))
                throw new UnauthorizedAccessException("You do not have access to the target folder.");
        }

        media.FolderId = request.TargetFolderId;
        media.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(cancellationToken);

        var ownerEmail = await db.Users
            .Where(u => u.Id == media.OwnerId)
            .Select(u => u.Email)
            .FirstOrDefaultAsync(cancellationToken);

        return new MediaDto
        {
            Id = media.Id,
            FileName = media.FileName,
            OriginalName = media.OriginalName,
            Url = media.Url,
            MimeType = media.MimeType,
            Size = media.Size,
            AltText = media.AltText,
            Caption = media.Caption,
            OwnerId = media.OwnerId,
            OwnerEmail = ownerEmail,
            FolderId = media.FolderId,
            CreatedAt = media.CreatedAt,
        };
    }
}
