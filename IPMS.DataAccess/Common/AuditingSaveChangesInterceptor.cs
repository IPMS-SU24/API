using IPMS.DataAccess.Common.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace IPMS.DataAccess.Common
{
    public class AuditingSaveChangesInterceptor : SaveChangesInterceptor
    {

        public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
        {
            var dbContext = eventData.Context;
            foreach (var entry in dbContext.ChangeTracker.Entries().Where(e => e.State == EntityState.Added || e.State == EntityState.Modified || e.State == EntityState.Deleted))
            {
                if (entry.Entity is BaseModel auditable)
                {
                    var now = DateTime.Now;
                    if (entry.State == EntityState.Added)
                    {
                        auditable.CreatedAt = now;
                        auditable.LastModified = now;
                    }
                    else if (entry.State == EntityState.Deleted)
                    {
                        entry.State = EntityState.Modified;
                        auditable.IsDeleted = true;
                    }
                    if (entry.State == EntityState.Modified)
                    {
                        auditable.LastModified = now;
                    }
                }
            }
            return base.SavingChanges(eventData, result);
        }

        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
        {
            var dbContext = eventData.Context;
            foreach (var entry in dbContext.ChangeTracker.Entries().Where(e => e.State == EntityState.Added || e.State == EntityState.Modified || e.State == EntityState.Deleted))
            {
                if (entry.Entity is BaseModel auditable)
                {
                    var now = DateTime.Now;
                    if (entry.State == EntityState.Added)
                    {
                        auditable.CreatedAt = now;
                        auditable.LastModified = now;
                    }
                    else if (entry.State == EntityState.Deleted)
                    {
                        entry.State = EntityState.Modified;
                        auditable.IsDeleted = true;
                    }
                    if (entry.State == EntityState.Modified)
                    {
                        auditable.LastModified = now;
                    }
                }
            }
            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }
    }

}
