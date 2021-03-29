using AirlineWeb.Dtos;
using AirlineWeb.Models;
using AutoMapper;

namespace AirlineWeb.Profiles
{
  public class FlightDetailProfile : Profile
  {
    public FlightDetailProfile()
    {
      CreateMap<FlightDetailCreateDto, FlightDetail>();
      CreateMap<FlightDetail, FlightDetailReadDto>();
      CreateMap<FlightDetailUpdateDto, FlightDetail>();
    }
  }
}