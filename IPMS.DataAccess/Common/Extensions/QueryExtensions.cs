using IPMS.DataAccess.Common.Models;
using System.Linq.Expressions;

namespace IPMS.DataAccess.Common.Extensions
{
    public static partial class QueryExtensions
    {
        public static IQueryable<T> GetEntityDeleted<T>(this IQueryable<T> source) where T : BaseModel
        {
            return source.Where(x => x.IsDeleted);
        }
        public static IQueryable<T> ApplySoftDeleteFilter<T>(this IQueryable<T> query) where T : BaseModel
        {
            var entityType = typeof(T);
            var parameter = Expression.Parameter(entityType, "e");

            var body = Expression.AndAlso(
                Expression.Equal(Expression.Property(parameter, nameof(BaseModel.IsDeleted)), Expression.Constant(false)),
                GetRelatedEntitiesFilter(entityType, parameter)
            );

            var lambda = Expression.Lambda<Func<T, bool>>(body, parameter);
            return query.Where(lambda);
        }

        private static Expression GetRelatedEntitiesFilter(Type entityType, ParameterExpression parameter)
        {
            Expression combinedExpression = Expression.Constant(true);

            var properties = entityType.GetProperties();
            foreach (var property in properties)
            {
                if (typeof(BaseModel).IsAssignableFrom(property.PropertyType))
                {
                    var relatedEntity = Expression.Property(parameter, property.Name);
                    var isDeletedProperty = Expression.Property(relatedEntity, nameof(BaseModel.IsDeleted));
                    var isNotDeleted = Expression.Equal(isDeletedProperty, Expression.Constant(false));

                    combinedExpression = Expression.AndAlso(combinedExpression, isNotDeleted);
                }
            }

            return combinedExpression;
        }
    }
}
