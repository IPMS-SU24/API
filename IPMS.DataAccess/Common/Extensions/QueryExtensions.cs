using IPMS.DataAccess.Common.Models;
using IPMS.DataAccess.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System.Linq.Expressions;

namespace IPMS.DataAccess.Common.Extensions
{
    public static partial class QueryExtensions
    {
        public static IQueryable<T> GetEntityDeleted<T>(this IQueryable<T> source) where T : class, IBaseModel
        {
            return source.Where(x => x.IsDeleted.Value);
        }
        public static void ApplySoftDeleteFilter(this IMutableEntityType mutableEntityType)
        {
            var entityType = mutableEntityType.ClrType;
            var parameter = Expression.Parameter(entityType, "e");

            var body = Expression.AndAlso(
                Expression.Equal(Expression.Property(parameter, nameof(IBaseModel.IsDeleted)), Expression.Constant(false, typeof(bool?))),
                GetRelatedEntitiesFilter(entityType, parameter, mutableEntityType)
            );

            var lambda = Expression.Lambda(body, parameter);
            mutableEntityType.SetQueryFilter(lambda);
        }

        private static Expression GetRelatedEntitiesFilter(Type entityType, ParameterExpression parameter, IMutableEntityType mutableEntityType)
        {
            Expression combinedExpression = Expression.Constant(true);

            var foreignKeys = mutableEntityType.GetForeignKeys();
            foreach (var foreignKey in foreignKeys)
            {
                var navigation = foreignKey.DependentToPrincipal;
                if (navigation != null)
                {
                    var relatedEntity = Expression.Property(parameter, navigation.Name);
                    var isDeletedProperty = Expression.Property(relatedEntity, nameof(IBaseModel.IsDeleted));
                    var isNotDeleted = Expression.Equal(isDeletedProperty, Expression.Constant(false, typeof(bool?)));

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
