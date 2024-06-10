using AutoMapper;
using IPMS.Business.Requests.IoTComponent;
using IPMS.DataAccess.Common.Enums;
using IPMS.DataAccess.Models;

namespace IPMS.API.MappingProfiles.IoT
{
    public class IoTProfile : Profile
    {
        public IoTProfile()
        {
            CreateMap<IoTModelRequest, ComponentsMaster>()
                .AfterMap((src, dest, context) =>
                {
                    dest.MasterType = (ComponentsMasterType) context.Items[nameof(ComponentsMaster.MasterType)];
                    dest.Status = BorrowedStatus.Pending;
                    dest.MasterId = Guid.Parse(context.Items[nameof(ComponentsMaster.MasterId)]!.ToString());
                });
        }
    }
}
