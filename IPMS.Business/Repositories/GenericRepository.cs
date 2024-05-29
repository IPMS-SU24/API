using IPMS.Business.Interfaces.Repositories;
using IPMS.DataAccess;
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
            
            return dbSet.AsQueryable();
        }

        public virtual async Task<TEntity?> GetByID(object id)
        {
            return await dbSet.FindAsync(id);
        }

        public virtual async Task Insert(TEntity entity)
        {
            await dbSet.AddAsync(entity);
        }

        public virtual void SoftDelete(TEntity deleteEntity)
        {
            deleteEntity.IsDeleted = true;
            Update(deleteEntity);
        }

        public virtual void Delete(TEntity entityToDelete)
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
    }
}
