﻿using IPMS.DataAccess.Common.Enums;

namespace IPMS.Business.Responses.Project
{
    public class GetProjectDetailResponse
    {
        public string GroupName { get; set; }
        public string TopicName { get; set; }
        public RequestStatus TopicStatus { get; set; }
        public List<MemberPrjDetail> Members { get; set; } = new();
        public List<IotBorrow> IotBorrows { get; set; }
    }

    public class MemberPrjDetail
    {
        public Guid Id { get; set; }
        public string StudentId { get; set; }   
        public string Name { get; set;}
        public bool isLeader { get; set; }
    }
    public class IotBorrow
    {
        public DateTime CreateAt { get; set; }
        public List<IotItem> Items { get; set; } = new();
    }
    public class IotItem
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string? Comment { get; set; }
        public int Quantity { get; set; }
        public Guid ComponentId { get; set; }
        public BorrowedStatus? Status { get; set; }
        public DateTime? ReturnedAt { get; set; }
        public int? MaxQuantityInTopic { get; set; }
    }
}
