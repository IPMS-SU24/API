using IPMS.DataAccess.Common.Models;

namespace IPMS.Business.Interfaces.Repositories
{
    public interface IGenericRepository<TEntity>
    {
        IQueryable<TEntity> Get();
        Task<TEntity?> GetByIDAsync(object id);
        Task InsertAsync(TEntity entity);
        Task InsertRangeAsync(IEnumerable<TEntity> entities);
        void Delete(TEntity deleteEntity);
        void HardDelete(TEntity deleteEntity);
        void Update(TEntity updateEntity);
        void Attach(TEntity updateEntity);
        Task LoadExplicitProperty(TEntity entity, string propName);
    }
}
