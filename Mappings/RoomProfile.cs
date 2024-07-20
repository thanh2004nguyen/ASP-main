using AutoMapper;
using Group5.Models;
using Group5.ViewModels;

namespace Group5.Mappings
{
    public class RoomProfile : Profile
    {
        public RoomProfile()
        {
            CreateMap<Room, RoomViewModel>()
                .ForMember(dst => dst.Admin, opt => opt.MapFrom(x => x.Admin.Email));

            CreateMap<RoomViewModel, Room>();
        }
    }
}
