using MediatR;
using Seems.Application.Content.Dtos;

namespace Seems.Application.ContentTypes.Queries.GetContentType;

public record GetContentTypeQuery(Guid Id) : IRequest<ContentTypeDto>;
