using AutoMapper;
using Group5.ViewModels;
using Group5.Data;

namespace Group5.Mappings
{
    public class UserProfile : Profile
    {
        public UserProfile()
        {
            CreateMap<Employee, UserViewModel>()
                .ForMember(dst => dst.UserName, opt => opt.MapFrom(x => x.Email));
            CreateMap<UserViewModel, Employee>();

            CreateMap<Employee, EmployeeViewModel>();
            CreateMap<EmployeeViewModel, Employee>();

            CreateMap<CreateEmployee, Employee>();
            CreateMap<Employee, CreateEmployee>();

        }
    }
}
