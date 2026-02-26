using MediatR;
using Seems.Application.Settings.Dtos;

namespace Seems.Application.Settings.Queries.GetSiteInfo;

public record GetSiteInfoQuery : IRequest<SiteInfoDto>;
