using MediatR;
using Microsoft.EntityFrameworkCore;
using Seems.Application.Common.Interfaces;
using Seems.Application.Media.Dtos;

namespace Seems.Application.Media.Queries.ListMediaFolders;

public class ListMediaFoldersHandler(
    IAppDbContext db,
    ICurrentUser currentUser) : IRequestHandler<ListMediaFoldersQuery, List<MediaFolderDto>>
{
    public async Task<List<MediaFolderDto>> Handle(ListMediaFoldersQuery request, CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId
            ?? throw new UnauthorizedAccessException("User not authenticated.");

        var isAdmin = currentUser.Roles.Contains("Admin");

        var query = db.MediaFolders.AsQueryable();
        if (!isAdmin)
            query = query.Where(f => f.OwnerId == userId);

        return await query
            .OrderBy(f => f.Name)
            .Select(f => new MediaFolderDto
            {
                Id = f.Id,
                Name = f.Name,
                OwnerId = f.OwnerId,
                ParentId = f.ParentId,
                ChildCount = f.Children.Count,
                MediaCount = f.Items.Count,
                CreatedAt = f.CreatedAt,
            })
            .ToListAsync(cancellationToken);
    }
}
