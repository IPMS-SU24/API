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
            //All Class Query
            var classesQuery = _unitOfWork.IPMSClassRepository.Get().Include(x => x.Semester)
                                                         .Where(x => x.Semester.ShortName == request.SemesterCode && x.LecturerId == lecturerId)
                                                         .Select(x => new
                                                         {
                                                             ClassCode = x.Name,
                                                             ClassId = x.Id,
                                                             ClassName = x.Description,
                                                             MaxMembers = x.MaxMember
                                                         });

            //Calculate enrolled Student (EmailConfirmed == true), total student, number of groups
            var studentQuery = _unitOfWork.StudentRepository.Get().Include(x => x.Information)
                                                                    .GroupBy(x => x.ClassId).Select(x => new
                                                                    {
                                                                        ClassId = x.Key,
                                                                        Enroll = x.Where(stu => stu.Information.EmailConfirmed).Count(),
                                                                        Total = x.Count(),
                                                                        GroupNum = x.Select(y => y.ProjectId).Distinct().Count()
                                                                    });
            var result = await classesQuery.Join(
                                studentQuery,
                                c => c.ClassId,
                                s => s.ClassId,
                                (@class, student) => new ClassInSemesterInfo
                                {
                                   ClassId = @class.ClassId,
                                   ClassName = @class.ClassName,
                                   ClassCode = @class.ClassCode,
                                   MaxMembers = @class.MaxMembers,
                                   Enroll = student.Enroll,
                                   Total = student.Total,
                                   GroupNum = student.GroupNum,
                                }
                                ).ToListAsync();
            if (result == null || !result.Any()) throw new DataNotFoundException();
            return new()
            {
                Classes = result
            };
        }
    }
}
