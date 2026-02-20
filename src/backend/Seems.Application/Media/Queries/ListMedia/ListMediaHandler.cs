using MediatR;
using Microsoft.EntityFrameworkCore;
using Seems.Application.Common.Interfaces;
using Seems.Application.Common.Models;
using Seems.Application.Media.Dtos;

namespace Seems.Application.Media.Queries.ListMedia;

public class ListMediaHandler(
    IAppDbContext db,
    ICurrentUser currentUser) : IRequestHandler<ListMediaQuery, PaginatedList<MediaDto>>
{
    public async Task<PaginatedList<MediaDto>> Handle(ListMediaQuery request, CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId
            ?? throw new UnauthorizedAccessException("User not authenticated.");

        var isAdmin = currentUser.Roles.Contains("Admin");

        var query = db.Media.AsQueryable();

        if (!isAdmin)
            query = query.Where(m => m.OwnerId == userId);

        query = query.Where(m => m.FolderId == request.FolderId);

        var total = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(m => m.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Join(db.Users,
                m => m.OwnerId,
                u => u.Id,
                (m, u) => new MediaDto
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
                    OwnerEmail = u.Email,
                    FolderId = m.FolderId,
                    CreatedAt = m.CreatedAt,
                })
            .ToListAsync(cancellationToken);

        return new PaginatedList<MediaDto>(items, total, request.Page, request.PageSize);
    }
}
