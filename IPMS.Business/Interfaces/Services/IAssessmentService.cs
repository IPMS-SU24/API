using IPMS.Business.Models;
using IPMS.Business.Responses.Assessment;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IPMS.Business.Interfaces.Services
{
    public interface IAssessmentService
    {
        Task<AssessmentSubmissionProjectResponse> GetAssessmentById(Guid assessmentId);
        Task<ValidationResultModel> GetAssessmentByIdValidators(Guid assessmentId);
    }
}
