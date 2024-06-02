using AutoMapper;
using IPMS.Business.Requests;
using IPMS.DataAccess.Common.Enums;
using IPMS.DataAccess.Models;

namespace IPMS.API.MappingProfiles
{
    public class RegisterBorrowIoTProfile : Profile
    {
        public RegisterBorrowIoTProfile()
        {
            CreateMap<BorrowIoTModelRequest, ComponentsMaster>()
                .AfterMap((src, dest, context) =>
                {
                    dest.MasterType = ComponentsMasterType.Project;
                    dest.Status = BorrowedStatus.Pending;
                    dest.MasterId = Guid.Parse(context.Items["MasterId"]!.ToString());
                });
        }
    }
}
