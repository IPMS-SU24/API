namespace IPMS.Business.Interfaces.Repositories
{
    public interface IGenericRepository<TEntity>
    {
        IQueryable<TEntity> Get();
        Task<TEntity?> GetByID(object id);
        Task Insert(TEntity entity);
        void Delete(TEntity deleteEntity);
        void HardDelete(TEntity deleteEntity);
        void Update(TEntity updateEntity);
    }
}
