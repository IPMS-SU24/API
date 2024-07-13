using IPMS.DataAccess.Common.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace IPMS.DataAccess.Common.Extensions
{
    public static partial class QueryExtensions
    {
        public static IQueryable<T> GetEntityDeleted<T>(this IQueryable<T> source) where T : BaseModel
        {
            return source.Where(x => x.IsDeleted);
        }
        public static IQueryable<T> ApplySoftDeleteFilter<T>(this IQueryable<T> query, IPMSDbContext context) where T : BaseModel
        {
            var entityType = typeof(T);
            var parameter = Expression.Parameter(entityType, "e");

            var body = Expression.AndAlso(
                Expression.Equal(Expression.Property(parameter, nameof(BaseModel.IsDeleted)), Expression.Constant(false)),
                GetRelatedEntitiesFilter(entityType, parameter, context)
            );

            var lambda = Expression.Lambda<Func<T, bool>>(body, parameter);
            return query.Where(lambda);
        }

        private static Expression GetRelatedEntitiesFilter(Type entityType, ParameterExpression parameter, IPMSDbContext context)
        {
            Expression combinedExpression = Expression.Constant(true);

            var entityTypeModel = context.Model.FindEntityType(entityType);
            if (entityTypeModel == null) return combinedExpression;

            var foreignKeys = entityTypeModel.GetForeignKeys();
            foreach (var foreignKey in foreignKeys)
            {
                var navigation = foreignKey.DependentToPrincipal;
                if (navigation != null)
                {
                    var relatedEntity = Expression.Property(parameter, navigation.Name);
                    var isDeletedProperty = Expression.Property(relatedEntity, nameof(BaseModel.IsDeleted));
                    var isNotDeleted = Expression.Equal(isDeletedProperty, Expression.Constant(false));

                    var foreignKeyProperty = foreignKey.Properties.FirstOrDefault();
                    if (foreignKeyProperty != null && Nullable.GetUnderlyingType(foreignKeyProperty.ClrType) != null)
                    {
                        var foreignKeyNotNull = Expression.NotEqual(Expression.Property(parameter, foreignKeyProperty.Name), Expression.Constant(null, foreignKeyProperty.ClrType));
                        isNotDeleted = Expression.AndAlso(foreignKeyNotNull, isNotDeleted);
                        var foreignKeyNull = Expression.Equal(Expression.Property(parameter, foreignKeyProperty.Name), Expression.Constant(null, foreignKeyProperty.ClrType));
                        isNotDeleted = Expression.Or(foreignKeyNull, isNotDeleted);
                    }
                    combinedExpression = Expression.AndAlso(combinedExpression, isNotDeleted);
                }
            }

            return combinedExpression;
        }
    }
}
