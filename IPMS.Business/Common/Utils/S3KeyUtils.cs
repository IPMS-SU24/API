namespace IPMS.Business.Common.Utils
{
    public static class S3KeyUtils
    {
        public static string? GetS3Key(string prefix, Guid entityId, string? fileName)
        {
            return !string.IsNullOrEmpty(fileName) ? string.Join("_", prefix, entityId, fileName) : null;
        }
    }
}
