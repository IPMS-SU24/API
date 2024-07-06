namespace IPMSBackgroundService.Exceptions
{
    public class SendMailFailException : Exception
    {
        public SendMailFailException() : base("Send mail fail") { }
        public SendMailFailException(Exception innerException) : base("Send mail fail", innerException) { }
    }
}
