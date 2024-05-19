﻿using System;
using IPMS.DataAccess.Common.Models;

namespace IPMS.DataAccess.Models
{
    public class Student : BaseModel
    {
        public decimal? ContributePercentage { get; set; }
        public decimal? FinalGrade { get; set; }
        public Guid? ProjectId { get; set; }
        public Guid? InformationId { get; set; }
        public Guid? ClassId { get; set; }
        public virtual IPMSUser? Information { get; set; }
        public virtual IPMSClass? Class { get; set; }
        public virtual Project? Project { get; set; }
    }
}
