using MediatR;
using Seems.Application.Media.Dtos;

namespace Seems.Application.Media.Commands.UploadMedia;

public record UploadMediaCommand(
    byte[] Content,
    string FileName,
    string ContentType,
    long Size,
    Guid? FolderId) : IRequest<MediaDto>;
