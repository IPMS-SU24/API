namespace IPMS.Business.Common.Exceptions
{
    public class CannotResetPasswordException : BaseBadRequestException
    {
        public CannotResetPasswordException(string[] errors) : base(errors)
        {
        }
    }
}
