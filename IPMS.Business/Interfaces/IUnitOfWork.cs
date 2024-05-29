﻿using IPMS.Business.Interfaces.Repositories;

namespace IPMS.Business.Interfaces
{
    public interface IUnitOfWork
    {
        IProjectRepository ProjectRepository { get; }
        IStudentRepository StudentRepository { get; }
        IClassTopicRepository ClassTopicRepository { get; }
        IProjectSubmissionRepository ProjectSubmissionRepository { get; }
        ISubmissionModuleRepository SubmissionModuleRepository { get; }
        IIPMSClassRepository IPMSClassRepository { get; }
        ISemesterRepository SemesterRepository { get; }
        ISyllabusRepository SyllabusRepository { get; }
        IAssessmentRepository AssessmentRepository { get; }
        ITopicRepository TopicRepository { get; }
        Task SaveChangesAsync();
    }
}
