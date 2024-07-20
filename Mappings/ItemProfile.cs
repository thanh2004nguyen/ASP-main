using AutoMapper;
using Group5.Models;
using Group5.ViewModels;

namespace Group5.Mappings
{
    public class ItemProfile: Profile
    {
        public ItemProfile()
        {
            CreateMap<StationeryItemViewModel, StationeryItem>();
            CreateMap<StationeryItem, StationeryItemViewModel>();

            CreateMap<NewStationeryRequest, NewStationeryRequestDto>();
            CreateMap<NewStationeryRequestDto, NewStationeryRequest>();
        }
    }
}
