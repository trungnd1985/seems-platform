using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Seems.Application.Media.Dtos;
using Seems.Domain.Interfaces;

namespace Seems.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MediaController(IRepository<Domain.Entities.Media> repository, IMapper mapper) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<MediaDto>>> List()
    {
        var items = await repository.GetAllAsync();
        return Ok(mapper.Map<IReadOnlyList<MediaDto>>(items));
    }
}
