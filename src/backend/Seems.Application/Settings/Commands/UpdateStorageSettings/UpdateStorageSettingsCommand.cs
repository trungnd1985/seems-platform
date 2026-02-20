using MediatR;
using Seems.Application.Settings.Dtos;

namespace Seems.Application.Settings.Commands.UpdateStorageSettings;

public record UpdateStorageSettingsCommand(StorageSettingsDto Settings) : IRequest;
