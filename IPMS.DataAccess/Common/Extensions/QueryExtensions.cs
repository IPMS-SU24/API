using IPMS.DataAccess.Common.Models;

namespace IPMS.DataAccess.Common.Extensions
{
    public static partial class QueryExtensions
    {
        public static IQueryable<T> GetEntityDeleted<T>(this IQueryable<T> source) where T : BaseModel
        {
            return source.Where(x => x.IsDeleted);
        }
    }
}
