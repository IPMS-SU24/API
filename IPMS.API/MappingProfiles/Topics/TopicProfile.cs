using AutoMapper;
using IPMS.Business.Requests.Topic;
using IPMS.DataAccess.Models;
using IPMS.DataAccess.Common.Enums;

namespace IPMS.API.MappingProfiles.Topics
{
    public class TopicProfile : Profile
    {
        public TopicProfile()
        {
            CreateMap<RegisterTopicRequest, Topic>()
                .ForMember(dest => dest.Name, opts => opts.MapFrom(src => src.TopicName))
                .ForMember(dest => dest.Detail, opts => opts.MapFrom(src => src.FileName))
                .AfterMap((src, dest) =>
                {
                    dest.Status = RequestStatus.Waiting;
                });

            CreateMap<LecturerRegisterTopicRequest, Topic>()
               .ForMember(dest => dest.Name, opts => opts.MapFrom(src => src.TopicName))
               .ForMember(dest => dest.Detail, opts => opts.MapFrom(src => src.FileName))
               .AfterMap((src, dest) =>
               {
                   dest.Status = RequestStatus.Approved;
               });
        }
    }
}
