using MediatR;
using Seems.Application.Settings.Dtos;

namespace Seems.Application.Settings.Commands.UpdateSiteInfo;

public record UpdateSiteInfoCommand(SiteInfoDto Settings) : IRequest;
