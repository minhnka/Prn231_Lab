using AutoMapper;
using BusinessObjects;
using Lab03.Controllers;

namespace Lab03
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Orchid -> OrchidDto
            CreateMap<Orchid, OrchidController.OrchidDto>()
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category != null ? src.Category.CategoryName : null));


            
            // CreateOrchidDto -> Orchid
            CreateMap<OrchidController.CreateOrchidDto, Orchid>();
            
            // UpdateOrchidDto -> Orchid
            CreateMap<OrchidController.UpdateOrchidDto, Orchid>();
        }
    }
}

