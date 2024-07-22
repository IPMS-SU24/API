namespace IPMS.Business.Common.Exceptions
{
    public class BaseBadRequestException : Exception
    {
        public string[] Errors { get; }
        public BaseBadRequestException(string message)
            : base(message)
        {
        }
        public BaseBadRequestException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
        public BaseBadRequestException(string[] errors)
            : base()
        {
            Errors = errors;
        }

    }
}
