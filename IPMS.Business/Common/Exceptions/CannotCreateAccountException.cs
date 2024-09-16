namespace IPMS.Business.Common.Exceptions
{
    public class CannotCreateAccountException : BackgroundJobException
    {
        public CannotCreateAccountException(string hashKey, string valueKey) : base("Cannot create student account", hashKey, valueKey) { }
    }
}
