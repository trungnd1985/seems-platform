using MediatR;
using Seems.Application.Media.Dtos;

namespace Seems.Application.Media.Queries.ListMediaFolders;

public record ListMediaFoldersQuery : IRequest<List<MediaFolderDto>>;
