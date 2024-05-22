namespace IPMS.Business.Interfaces.Repositories
{
    public interface IGenericRepository<TEntity>
    {
        IEnumerable<TEntity> Get();
        TEntity GetByID(object id);
        void Insert(TEntity entity);
        void Delete(object id);
        void Delete(TEntity entityToDelete);
        void Update(TEntity entityToUpdate);
    }
}
