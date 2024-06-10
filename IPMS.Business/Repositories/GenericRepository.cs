using IPMS.Business.Interfaces.Repositories;
using IPMS.DataAccess;
using IPMS.DataAccess.Common.Extensions;
using IPMS.DataAccess.Common.Models;
using Microsoft.EntityFrameworkCore;

namespace IPMS.Business.Repository
{
    public class GenericRepository<TEntity> : IGenericRepository<TEntity> where TEntity : BaseModel
    {
        internal IPMSDbContext _context;
        internal DbSet<TEntity> dbSet;

        public GenericRepository(IPMSDbContext context)
        {
            this._context = context;
            this.dbSet = context.Set<TEntity>();
        }

        public virtual IQueryable<TEntity> Get()
        {
            return dbSet.AsNoTracking();
        }

        public virtual async Task<TEntity?> GetByIDAsync(object id)
        {
            var result =  await dbSet.FindAsync(id);
            if (result != null && result.IsDeleted) return null;
            return await dbSet.FindAsync(id);
        }

        public virtual async Task InsertAsync(TEntity entity)
        {
            await dbSet.AddAsync(entity);
        }

        public virtual void Delete(TEntity deleteEntity)
        {
            deleteEntity.IsDeleted = true;
            Update(deleteEntity);
        }

        public virtual void HardDelete(TEntity entityToDelete)
        {
            if (_context.Entry(entityToDelete).State == EntityState.Detached)
            {
                dbSet.Attach(entityToDelete);
            }
            dbSet.Remove(entityToDelete);
        }

        public virtual void Update(TEntity entityToUpdate)
        {
            dbSet.Attach(entityToUpdate);
            _context.Entry(entityToUpdate).State = EntityState.Modified;
        }

        public async Task InsertRangeAsync(IEnumerable<TEntity> entities)
        {
            await dbSet.AddRangeAsync(entities);
        }

        public async Task LoadExplicitProperty(TEntity entity, string propName)
        {
            await _context.Entry(entity).Navigation(propName).LoadAsync();
        }
    }
}
