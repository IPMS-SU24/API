namespace IPMS.Business.Common.Exceptions
{
    public class CannotImportStudentException : BaseBadRequestException
    {
        public CannotImportStudentException(Exception innerException) : base("Import File is not exist or cannot map to student data", innerException) { }
    }
}
