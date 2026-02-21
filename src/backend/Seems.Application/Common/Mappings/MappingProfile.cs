using System.Text.Json;
using AutoMapper;
using Seems.Application.Content.Dtos;
using Seems.Application.Identity.Dtos;
using Seems.Application.Media.Dtos;
using Seems.Application.Pages.Dtos;
using Seems.Application.Templates.Dtos;
using Seems.Application.Themes.Dtos;
using Seems.Domain.Entities;
using Seems.Domain.Entities.Identity;

namespace Seems.Application.Common.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Page, PageDto>();
        CreateMap<SlotMapping, SlotMappingDto>();
        CreateMap<ContentType, ContentTypeDto>();
        CreateMap<ContentItem, ContentItemDto>();
        CreateMap<Template, TemplateDto>()
            .ForMember(d => d.Slots, opt => opt.MapFrom(s =>
                JsonSerializer.Deserialize<List<TemplateSlotDef>>(
                    string.IsNullOrEmpty(s.Slots) ? "[]" : s.Slots) ?? new List<TemplateSlotDef>()))
            .ForMember(d => d.ThemeExists, opt => opt.Ignore());
        CreateMap<Theme, ThemeDto>();
        CreateMap<Domain.Entities.Media, MediaDto>();
        CreateMap<AppUser, UserDto>()
            .ForMember(d => d.Roles, opt => opt.Ignore());
    }
}
