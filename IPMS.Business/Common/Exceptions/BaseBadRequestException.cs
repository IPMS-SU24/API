namespace IPMS.Business.Common.Exceptions
{
    public class BaseBadRequestException : Exception
    {
        private string[] _errors;
        public string[] Errors
        {
            get
            {
                return _errors ?? Array.Empty<string>();
            }
        }
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
            _errors = errors;
        }

    }
}
