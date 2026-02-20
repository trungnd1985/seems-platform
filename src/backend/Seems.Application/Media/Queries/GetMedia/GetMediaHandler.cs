using MediatR;
using Microsoft.EntityFrameworkCore;
using Seems.Application.Common.Interfaces;
using Seems.Application.Media.Dtos;

namespace Seems.Application.Media.Queries.GetMedia;

public class GetMediaHandler(
    IAppDbContext db,
    ICurrentUser currentUser) : IRequestHandler<GetMediaQuery, MediaDto>
{
    public async Task<MediaDto> Handle(GetMediaQuery request, CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId
            ?? throw new UnauthorizedAccessException("User not authenticated.");

        var media = await db.Media
            .Join(db.Users,
                m => m.OwnerId,
                u => u.Id,
                (m, u) => new { m, OwnerEmail = u.Email })
            .FirstOrDefaultAsync(x => x.m.Id == request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Media {request.Id} not found.");

        if (media.m.OwnerId != userId && !currentUser.Roles.Contains("Admin"))
            throw new UnauthorizedAccessException("You do not have access to this file.");

        return new MediaDto
        {
            Id = media.m.Id,
            FileName = media.m.FileName,
            OriginalName = media.m.OriginalName,
            Url = media.m.Url,
            MimeType = media.m.MimeType,
            Size = media.m.Size,
            AltText = media.m.AltText,
            Caption = media.m.Caption,
            OwnerId = media.m.OwnerId,
            OwnerEmail = media.OwnerEmail,
            FolderId = media.m.FolderId,
            CreatedAt = media.m.CreatedAt,
        };
    }
}
