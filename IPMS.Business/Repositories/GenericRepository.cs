﻿using IPMS.Business.Interfaces.Repositories;
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
            var technicalId = Guid.Parse(id.ToString());
            var result = await dbSet.Where(x=>x.Id == technicalId).FirstOrDefaultAsync();
            return result;
        }

        public virtual async Task InsertAsync(TEntity entity)
        {
            await dbSet.AddAsync(entity);
        }

        public virtual void Delete(TEntity deleteEntity)
        {
            dbSet.Remove(deleteEntity);
        }
        public virtual void DeleteRange(IEnumerable<TEntity> deleteEntities)
        {
            dbSet.RemoveRange(deleteEntities);
        }

        public virtual void Update(TEntity entityToUpdate)
        {
            _context.Entry(entityToUpdate).State = EntityState.Modified;
        }

        public async Task InsertRangeAsync(IEnumerable<TEntity> entities)
        {
            await dbSet.AddRangeAsync(entities);
        }

        public async Task LoadExplicitProperty(TEntity entity, string propName)
        {
            var isTracked = _context.Entry(entity).State != EntityState.Detached;
            if (!isTracked)
            {
                _context.Attach(entity);
            }
            await _context.Entry(entity).Navigation(propName).LoadAsync();
            if(!isTracked)
            {
                _context.Entry(entity).State = EntityState.Detached;
            }
        }

        public void Attach(TEntity updateEntity)
        {
            dbSet.Attach(updateEntity);
        }
    }
}
