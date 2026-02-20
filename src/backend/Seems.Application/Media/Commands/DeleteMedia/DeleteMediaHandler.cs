using MediatR;
using Seems.Application.Common.Interfaces;
using Seems.Domain.Interfaces;

namespace Seems.Application.Media.Commands.DeleteMedia;

public class DeleteMediaHandler(
    IAppDbContext db,
    IStorageProviderFactory storageFactory,
    ICurrentUser currentUser) : IRequestHandler<DeleteMediaCommand>
{
    public async Task Handle(DeleteMediaCommand request, CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId
            ?? throw new UnauthorizedAccessException("User not authenticated.");

        var media = await db.Media.FindAsync([request.Id], cancellationToken)
            ?? throw new KeyNotFoundException($"Media {request.Id} not found.");

        if (media.OwnerId != userId && !currentUser.Roles.Contains("Admin"))
            throw new UnauthorizedAccessException("You do not have access to this file.");

        var provider = await storageFactory.GetCurrentAsync(cancellationToken);
        await provider.DeleteAsync(media.StorageKey, cancellationToken);

        db.Media.Remove(media);
        await db.SaveChangesAsync(cancellationToken);
    }
}
