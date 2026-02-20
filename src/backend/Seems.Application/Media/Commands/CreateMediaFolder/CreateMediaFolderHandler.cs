using MediatR;
using Microsoft.EntityFrameworkCore;
using Seems.Application.Common.Interfaces;
using Seems.Application.Media.Dtos;
using Seems.Domain.Entities;

namespace Seems.Application.Media.Commands.CreateMediaFolder;

public class CreateMediaFolderHandler(
    IAppDbContext db,
    ICurrentUser currentUser) : IRequestHandler<CreateMediaFolderCommand, MediaFolderDto>
{
    public async Task<MediaFolderDto> Handle(CreateMediaFolderCommand request, CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId
            ?? throw new UnauthorizedAccessException("User not authenticated.");

        if (request.ParentId.HasValue)
        {
            var parent = await db.MediaFolders.FindAsync([request.ParentId.Value], cancellationToken)
                ?? throw new KeyNotFoundException($"Parent folder {request.ParentId} not found.");

            if (parent.OwnerId != userId && !currentUser.Roles.Contains("Admin"))
                throw new UnauthorizedAccessException("You do not have access to the parent folder.");
        }

        var duplicate = await db.MediaFolders.AnyAsync(
            f => f.OwnerId == userId && f.ParentId == request.ParentId && f.Name == request.Name,
            cancellationToken);

        if (duplicate)
            throw new InvalidOperationException($"A folder named '{request.Name}' already exists here.");

        var folder = new MediaFolder
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            OwnerId = userId,
            ParentId = request.ParentId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };

        db.MediaFolders.Add(folder);
        await db.SaveChangesAsync(cancellationToken);

        return new MediaFolderDto
        {
            Id = folder.Id,
            Name = folder.Name,
            OwnerId = folder.OwnerId,
            ParentId = folder.ParentId,
            ChildCount = 0,
            MediaCount = 0,
            CreatedAt = folder.CreatedAt,
        };
    }
}
