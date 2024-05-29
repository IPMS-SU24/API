namespace IPMS.API.Responses
{
    public class PaginationResponse<TData> : IPMSResponse<TData> where TData : class
    {
        public int TotalPage { get; set; } = 1;
        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } =  10;
    }
}
