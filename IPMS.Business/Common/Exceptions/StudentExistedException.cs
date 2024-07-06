namespace IPMS.Business.Common.Exceptions
{
    public class StudentExistedException : Exception
    {
        public StudentExistedException() : base("Student is existed in class")
        {
            
        }
    }
}
