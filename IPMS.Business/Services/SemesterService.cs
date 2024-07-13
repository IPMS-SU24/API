using IPMS.Business.Common.Exceptions;
using IPMS.Business.Common.Utils;
using IPMS.Business.Interfaces;
using IPMS.Business.Interfaces.Services;
using IPMS.Business.Requests.Semester;
using IPMS.Business.Responses.Semester;
using IPMS.DataAccess.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace IPMS.Business.Services
{
    public class SemesterService : ISemesterService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<IPMSUser> _userManager;

        public SemesterService(IUnitOfWork unitOfWork, UserManager<IPMSUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
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
            var classesQuery = await _unitOfWork.IPMSClassRepository.Get().Include(x => x.Semester)
                                                         .Where(x => x.Semester.ShortName == request.SemesterCode && x.LecturerId == lecturerId)
                                                         .Select(x => new
                                                         {
                                                             ClassCode = x.ShortName,
                                                             ClassId = x.Id,
                                                             ClassName = x.Name,
                                                             MaxMembers = x.MaxMember
                                                         }).ToListAsync();

            //Calculate enrolled Student (EmailConfirmed == true), total student, number of groups
            var studentQuery = await _unitOfWork.StudentRepository.Get().Include(x => x.Information)
                                                                    .GroupBy(x => x.ClassId).Select(x => new
                                                                    {
                                                                        ClassId = x.Key,
                                                                        Enroll = x.Where(stu => stu.Information.EmailConfirmed).Count(),
                                                                        Total = x.Count(),
                                                                        GroupNum = x.Where(x=>x.ProjectId != null).Select(y => y.ProjectId).Distinct().Count()
                                                                    }).ToListAsync();
            var result = new List<ClassInSemesterInfo>();
            foreach (var @class in classesQuery)
            {
                var studentInClass = studentQuery.Where(x => x.ClassId == @class.ClassId).ToList();
                if (!studentInClass.Any()) studentInClass.Add(new
                {
                    @class.ClassId,
                    Enroll = 0,
                    Total = 0,
                    GroupNum = 0
                });
                foreach (var student in studentInClass)
                {
                    result.Add(new ClassInSemesterInfo
                    {
                        ClassId = @class.ClassId,
                        ClassName = @class.ClassName,
                        ClassCode = @class.ClassCode,
                        MaxMembers = @class.MaxMembers,
                        Enroll = student.Enroll,
                        GroupNum = student.GroupNum,
                        Total = student.Total
                    });
                }
            }
            if (result == null || !result.Any()) throw new DataNotFoundException();
            return new()
            {
                Classes = result
            };
        }

        public async Task<GetCurrentSemesterResponse> GetCurrentSemester()
        {
            var currentSemester  = (await CurrentSemesterUtils.GetCurrentSemester(_unitOfWork)).CurrentSemester;
            return new GetCurrentSemesterResponse()
            {
                Code = currentSemester.ShortName,
                Name = currentSemester.Name,
                StartDate = currentSemester.StartDate,
                EndDate = currentSemester.EndDate
            };
        }

        public async Task<GetLecturerInSemesterResponse> GetLecturerInSemester(GetLecturerInSemesterRequest request)
        {
            GetLecturerInSemesterResponse lecturers = new GetLecturerInSemesterResponse();
            var semester = await _unitOfWork.SemesterRepository.Get().Where(s => s.ShortName.Equals(request.SemesterCode)).Include(s => s.Classes).FirstOrDefaultAsync();
            if (semester == null || semester.Classes.Count == 0)
            {
                return lecturers;
            }
            var lecturerIds = semester.Classes.DistinctBy(s => s.LecturerId).Select(c => c.LecturerId).ToList();
            List<IPMSUser> users = _userManager.Users.ToList();
            lecturers.Lecturers = lecturerIds.Select(l => new LecturerInfo { 
                Id = (Guid)l,
                Name = users.FirstOrDefault(u => u.Id.Equals(l)).FullName
            });

            return lecturers;
        }
    }
}
