using AutoMapper;

namespace Homecloud.Models.Profiles
{
    public class OrganizationProfile : Profile
    {
        public OrganizationProfile()
        {
            CreateMap<Homecloud.Models.Devops.OrganizationProject, Homecloud.Models.DevopsApi.OrganizationProject>();
        }
    }
}