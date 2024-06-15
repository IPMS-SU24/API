using IPMS.Business.Interfaces;
using IPMS.Business.Interfaces.Services;
using IPMS.Business.Responses.Semester;
using IPMS.Business.Common.Exceptions;
using Microsoft.EntityFrameworkCore;
using IPMS.Business.Requests.Semester;

namespace IPMS.Business.Services
{
    public class SemesterService : ISemesterService
    {
        private readonly IUnitOfWork _unitOfWork;
        public SemesterService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<GetAllSemestersResponse> GetAllSemesters()
        {
            var semesters =  await _unitOfWork.SemesterRepository.Get().Select(x=>new SemesterInfo
            {
                Code = x.ShortName,
                Name = x.Name
            }).ToListAsync();
            if(semesters == null || !semesters.Any())
            {
                throw new DataNotFoundException();
            }
            return new GetAllSemestersResponse
            {
                Semesters = semesters
            };
        }

        public async Task<GetClassInfoInSemesterResponse> GetClassesInSemester(Guid lecturerId, GetClassInfoInSemesterRequest request)
        {
            //Get All Class
            var classes = await _unitOfWork.IPMSClassRepository.Get().Include(x => x.Semester).Include(x=>x.Students)
                                                         .Where(x => x.Semester.ShortName == request.SemesterCode && x.LecturerId == lecturerId)
                                                         .Select(x => new ClassInSemesterInfo
                                                         {
                                                             ClassCode = x.Name,
                                                             ClassId = x.Id,
                                                             ClassName = x.Description,
                                                             MaxMembers = x.MaxMember,
                                                             Total = x.Students.Count,
                                                             GroupNum = x.Students.Select(x=>x.ProjectId).Distinct().Count()
                                                         })
                                                         .ToListAsync();
            if(classes == null || !classes.Any()) { throw new DataNotFoundException(); }
            //Calculate enrolled Student (EmailConfirmed == true)
            var studentCount =_unitOfWork.StudentRepository.Get().Include(x => x.Information);
            throw new NotImplementedException();
        }
    }
}
