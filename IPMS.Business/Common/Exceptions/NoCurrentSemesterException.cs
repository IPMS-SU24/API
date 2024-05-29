namespace IPMS.Business.Common.Exceptions
{
    public class NoCurrentSemesterException : Exception
    {
        public NoCurrentSemesterException()
            : base("No Current Semester Error")
        {
        }
    }
}
