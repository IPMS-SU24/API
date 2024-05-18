namespace IPMS.API.Responses
{
    public class PaginationResponse<TData> : IPMSResponse<TData> where TData : class
    {
        public int TotalPage = 1;
        public int CurrentPage = 1;
        public int PageSize = 10;
    }
}
