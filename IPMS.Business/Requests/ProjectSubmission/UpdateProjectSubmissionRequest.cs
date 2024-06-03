﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IPMS.Business.Requests.ProjectSubmission
{
    public class UpdateProjectSubmissionRequest
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public DateTime SubmissionDate { get; set; }
        public Guid? SubmissionModuleId { get; set; }
    }
}
