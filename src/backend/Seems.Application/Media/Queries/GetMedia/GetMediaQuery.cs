using MediatR;
using Seems.Application.Media.Dtos;

namespace Seems.Application.Media.Queries.GetMedia;

public record GetMediaQuery(Guid Id) : IRequest<MediaDto>;
