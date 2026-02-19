using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Seems.Application.Content.Dtos;
using Seems.Domain.Entities;
using Seems.Domain.Interfaces;

namespace Seems.Api.Controllers;

[ApiController]
[Route("api/content-types")]
[Authorize]
public class ContentTypesController(IRepository<ContentType> repository, IUnitOfWork unitOfWork, IMapper mapper)
    : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ContentTypeDto>>> List()
    {
        var items = await repository.GetAllAsync();
        return Ok(mapper.Map<IReadOnlyList<ContentTypeDto>>(items));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ContentTypeDto>> Get(Guid id)
    {
        var item = await repository.GetByIdAsync(id);
        return item is null ? NotFound() : Ok(mapper.Map<ContentTypeDto>(item));
    }

    [HttpPost]
    public async Task<ActionResult<ContentTypeDto>> Create([FromBody] ContentTypeDto dto)
    {
        var entity = new ContentType
        {
            Id = Guid.NewGuid(),
            Key = dto.Key,
            Name = dto.Name,
            Schema = dto.Schema,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };

        await repository.AddAsync(entity);
        await unitOfWork.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = entity.Id }, mapper.Map<ContentTypeDto>(entity));
    }
}
