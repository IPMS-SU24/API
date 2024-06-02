using AutoMapper;
using IPMS.Business.Responses.ProjectDashboard;
using IPMS.DataAccess.Models;

namespace IPMS.API.MappingProfiles
{
    public class BorrowedIoTProfile : Profile
    {
        public BorrowedIoTProfile()
        {
            CreateMap<ComponentsMaster, BorrowIoTComponentInformation>()
                .ForMember(x => x.Id, opt => opt.MapFrom(src => src.ComponentId))
                .ForMember(x => x.Name, opt => opt.MapFrom(src => src.Component!.Name));
        }
    }
}
