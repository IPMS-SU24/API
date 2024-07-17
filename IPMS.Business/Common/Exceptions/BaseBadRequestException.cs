namespace IPMS.Business.Common.Exceptions
{
    public class BaseBadRequestException : Exception
    {
        public BaseBadRequestException(string message)
            : base(message)
        {
        }
        public BaseBadRequestException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
