using AutoMapper;
using IPMS.Business.Requests.Report;
using IPMS.Business.Responses.Report;
using IPMS.DataAccess.Common.Enums;
using IPMS.DataAccess.Models;

namespace IPMS.API.MappingProfiles.Report
{
    public class ReportProfile : Profile
    {
        public ReportProfile()
        {
            CreateMap<ReportType, ReportTypeResponse>();
            CreateMap<SendReportRequest, DataAccess.Models.Report>()
                .ForMember(x=>x.Title, opt => opt.MapFrom(src=>src.ReportTitle))
                .ForMember(x=>x.Content, opt => opt.MapFrom(src=>src.ReportDescription))
                .AfterMap((src, dest, context) =>
                {
                    dest.ReporterId = Guid.Parse(context.Items["ReporterId"]!.ToString());
                });
        }
    }
}
