using MediatR;
using Seems.Application.Content.Dtos;

namespace Seems.Application.ContentTypes.Queries.ListContentTypes;

public record ListContentTypesQuery : IRequest<IReadOnlyList<ContentTypeDto>>;
