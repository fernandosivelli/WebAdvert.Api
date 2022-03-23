using AutoMapper;
using WebAdvert.Api.Domain.Models;
using WebAdvert.Api.Models;

namespace WebAdvert.Api.Service
{
    public class AdvertProfile : Profile
    {
        public AdvertProfile()
        {
            CreateMap<AdvertModel, AdvertDbModel>();
        }
    }
}
