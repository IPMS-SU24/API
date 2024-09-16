namespace IPMS.Business.Common.Exceptions
{
    public class BackgroundJobException : BaseBadRequestException
    {
        public string HashKey { get; init; } = string.Empty;
        public string ValueKey { get; init; } = string.Empty;
        public BackgroundJobException(string message) : base(message)
        {
        }

        public BackgroundJobException(string[] errors) : base(errors)
        {
        }

        public BackgroundJobException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public BackgroundJobException(string message, string hashKey, string valueKey) : base(message)
        {
            HashKey = hashKey;
            ValueKey = valueKey;
        }
    }
}
