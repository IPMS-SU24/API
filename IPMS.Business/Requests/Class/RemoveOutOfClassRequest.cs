namespace IPMS.Business.Requests.Class
{
    public class RemoveOutOfClassRequest
    {
        public Guid ClassId { get; set; }
        /// <summary>
        /// AccountId :))
        /// </summary>
        public Guid StudentId { get; set; }
    }
}
