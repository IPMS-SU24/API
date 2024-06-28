
using IPMS.Business.Common.Utils;
using IPMS.Business.Interfaces;
using IPMS.Business.Interfaces.Services;
using IPMS.Business.Models;
using IPMS.Business.Requests.SubmissionModule;
using IPMS.DataAccess.Models;
using Microsoft.EntityFrameworkCore;
using System.Xml.Linq;

namespace IPMS.Business.Services
{
    public class SubmissionModuleService : ISubmissionModuleService
    {
        private readonly IUnitOfWork _unitOfWork;
        private List<SubmissionModule> _submissionModules = new();
        private Guid _currentSemesterId;
        public SubmissionModuleService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<ValidationResultModel> ConfigureSubmissionModuleValidator(ConfigureSubmissionModuleRequest request, Guid currentUserId)
        {
            var result = new ValidationResultModel
            {
                Message = "Operation did not successfully"
            };
            bool isPerEqZero = request.SubmissionModules.Any(sm => sm.Percentage == 0);
            if (isPerEqZero) // percentage <= 0
            {
                result.Message = "Cannot set percentage lower or equals 0%";
                return result;
            }

            decimal isPerSumEqHundred = request.SubmissionModules.Sum(sm => sm.Percentage);
            if (isPerSumEqHundred != 100) // sub percentage != 100
            {
                result.Message = "Sum of percentage is different 100%";
                return result;
            }

            bool isStartGreaterEnd = request.SubmissionModules.Any(sm => sm.StartDate >= sm.EndDate);
            if (isStartGreaterEnd) // start date > end date
            {
                result.Message = "Start Date cannot equal or greater than End Date";
                return result;
            }

            bool isEndLessNow = request.SubmissionModules.Any(sm => sm.EndDate <= DateTime.Now 
                    || (sm.EndDate.Date == DateTime.Now.Date && sm.EndDate.Hour <= DateTime.Now.Hour)); // compare for hour

            if (isEndLessNow) // end date hour <= now hour
            {
                result.Message = "End Date must greater than now";
                return result;
            }

            bool isNameNull = request.SubmissionModules.Any(sm => sm.ModuleName == null || sm.ModuleName.Trim() == "");
            if (isNameNull) // module name null
            {
                result.Message = "Module Name cannot be null";
                return result;
            }

            _currentSemesterId = (await CurrentSemesterUtils.GetCurrentSemester(_unitOfWork)).CurrentSemester!.Id;

            _submissionModules = await _unitOfWork.SubmissionModuleRepository.Get().Where(sm => sm.AssessmentId.Equals(request.AssessmentId) 
                                            && sm.SemesterId.Equals(_currentSemesterId) 
                                            && sm.LectureId.Equals(currentUserId)).ToListAsync();
            foreach (var submissionModule in request.SubmissionModules) { 
                if (submissionModule.Description == null)
                {
                    submissionModule.Description = "";
                }
                if (submissionModule.ModuleId == null) // case create new module
                {
                    submissionModule.ModuleId = Guid.Empty;
                }

                if (submissionModule.ModuleId == Guid.Empty && submissionModule.IsDeleted == true) // submission module is not existed
                {
                    result.Message = "Cannot delete not exist module";
                    return result;
                }

                if (submissionModule.ModuleId != Guid.Empty)
                {
                    var isExisted = _submissionModules.FirstOrDefault(sm => sm.Equals(submissionModule.ModuleId));
                    if (isExisted == null) // submission module is not existed
                    {
                        result.Message = "Module is not existed to update";
                        return result;
                    }
                }

            }
            result.Message = string.Empty;
            result.Result = true;
            return result;
        }
        public async Task ConfigureSubmissionModule(ConfigureSubmissionModuleRequest request, Guid currentUserId)
        {
            foreach (var submissionModule in request.SubmissionModules)
            {
                if (submissionModule.ModuleId == Guid.Empty) // create
                {
                    Guid id = Guid.NewGuid();
                    SubmissionModule subModule = new SubmissionModule
                    {
                        Id = id,
                        Name = submissionModule.ModuleName,
                        Description = submissionModule.Description,
                        Percentage = submissionModule.Percentage,
                        StartDate = submissionModule.StartDate,
                        EndDate = submissionModule.EndDate,
                        SemesterId = _currentSemesterId,
                        AssessmentId = request.AssessmentId,
                        LectureId = currentUserId,
                    };
                    await _unitOfWork.SubmissionModuleRepository.InsertAsync(subModule);

                } else // update
                {
                    SubmissionModule subModule = _submissionModules.FirstOrDefault(sm => sm.Id.Equals(submissionModule.ModuleId));
                    subModule.Name = submissionModule.ModuleName;
                    subModule.Description = submissionModule.Description;
                    subModule.Percentage = submissionModule.Percentage;
                    subModule.StartDate = submissionModule.StartDate;
                    subModule.EndDate = submissionModule.EndDate;
                    subModule.IsDeleted = submissionModule.IsDeleted;
                    _unitOfWork.SubmissionModuleRepository.Update(subModule);

                }
            }

            await _unitOfWork.SaveChangesAsync();
        }
    }
}
