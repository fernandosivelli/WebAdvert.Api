using AutoMapper;
using WebAdvert.Api.Domain.Models;
using WebAdvert.Api.Models;

namespace WebAdvert.Api.Services
{
    public class AdvertProfile : Profile
    {
        public AdvertProfile()
        {
            CreateMap<AdvertModel, AdvertDbModel>();
        }
    }
}
