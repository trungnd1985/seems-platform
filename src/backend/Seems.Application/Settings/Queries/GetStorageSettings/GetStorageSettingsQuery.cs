using MediatR;
using Seems.Application.Settings.Dtos;

namespace Seems.Application.Settings.Queries.GetStorageSettings;

public record GetStorageSettingsQuery : IRequest<StorageSettingsDto>;
