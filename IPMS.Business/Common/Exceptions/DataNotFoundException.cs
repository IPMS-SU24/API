namespace IPMS.Business.Common.Exceptions
{
    public class DataNotFoundException : Exception
    {
        public DataNotFoundException()
            : base(string.Empty)
        {
        }

        public DataNotFoundException(string message)
            : base(message)
        {
        }

        public DataNotFoundException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public DataNotFoundException(string name, object key)
            : base($"Data with \"{name}\" ({key}) is not existed")
        {
        }
    }
}
