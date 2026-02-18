using MediatR;
using Seems.Application.Content.Dtos;

namespace Seems.Application.Content.Queries.GetContentItem;

public record GetContentItemQuery(Guid Id) : IRequest<ContentItemDto?>;
