namespace IPMS.Business.Common.Exceptions
{
    public class CannotCreateAccountException : BaseBadRequestException
    {
        public CannotCreateAccountException() : base("Cannot create student account") { }
    }
}
