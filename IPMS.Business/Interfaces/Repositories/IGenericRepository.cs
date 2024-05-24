namespace IPMS.Business.Interfaces.Repositories
{
    public interface IGenericRepository<TEntity>
    {
        IQueryable<TEntity> Get();
        Task<TEntity?> GetByID(object id);
        Task Insert(TEntity entity);
        void SoftDelete(TEntity deleteEntity);
        void Delete(TEntity deleteEntity);
        void Update(TEntity updateEntity);
    }
}
