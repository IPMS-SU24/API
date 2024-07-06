namespace IPMS.Business.Common.Exceptions
{
    public class CannotCreateAccountException : Exception
    {
        public CannotCreateAccountException() : base("Cannot create student account") { }
    }
}
