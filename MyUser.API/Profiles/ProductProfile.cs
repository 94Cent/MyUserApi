using AutoMapper;
using MyUser.API.Models;
using MyUser.API.Models.Dto;

namespace MyUser.API.Profiles
{
    public class ProductProfile : Profile
    {
        public ProductProfile()
        {
            CreateMap<Product, ProductDto>().ReverseMap();
        }
    }
}
