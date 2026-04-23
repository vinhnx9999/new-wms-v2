using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Newtonsoft.Json;
using WMSSolution.WMS.Entities.Models.ActionLog;

namespace WMSSolution.Core.DBContext
{

    /// <summary>
    /// Audit log using EF core Interceptor
    /// </summary>
    public class AuditableEntityInterceptor(IHttpContextAccessor httpContextAccessor) : SaveChangesInterceptor
    {
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

        /// <summary>
        /// overide method for config
        /// </summary>
        /// <param name="eventData"></param>
        /// <param name="result"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
        {
            var context = eventData.Context;
            if (context == null) return await base.SavingChangesAsync(eventData, result, cancellationToken);

            var auditEntries = new List<AuditLogEntity>();
            var userName = _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "System";

            foreach (var entry in context.ChangeTracker.Entries())
            {
                if (entry.Entity is AuditLogEntity || entry.State == EntityState.Detached || entry.State == EntityState.Unchanged)
                    continue;

                var auditEntry = new AuditLogEntity
                {
                    TableName = entry.Entity.GetType().Name,
                    Action = entry.State.ToString(),
                    //  Id = 1, // This will be set by the database
                    RecordId = 1.ToString(), // Temporary, will be set below
                };

                var oldValues = new Dictionary<string, object>();
                var newValues = new Dictionary<string, object>();

                foreach (var property in entry.Properties)
                {
                    var propertyName = property.Metadata.Name;
                    if (property.IsTemporary)
                    {
                        continue;
                    }


                    if (property.Metadata.IsPrimaryKey())
                    {
                        auditEntry.RecordId = property.CurrentValue?.ToString() ?? string.Empty;
                        continue;
                    }

                    switch (entry.State)
                    {
                        case EntityState.Added:
                            newValues[propertyName] = property.CurrentValue ?? string.Empty;
                            break;

                        case EntityState.Deleted:
                            oldValues[propertyName] = property.OriginalValue ?? string.Empty;
                            break;

                        case EntityState.Modified:
                            if (property.IsModified)
                            {
                                oldValues[propertyName] = property.OriginalValue ?? string.Empty;
                                newValues[propertyName] = property.CurrentValue ?? string.Empty;
                            }
                            break;
                    }
                }

                auditEntry.OldValues = oldValues.Count == 0 ? null : JsonConvert.SerializeObject(oldValues);
                auditEntry.NewValues = newValues.Count == 0 ? null : JsonConvert.SerializeObject(newValues);

                auditEntries.Add(auditEntry);
            }

            if (auditEntries.Count > 0)
            {
                await context.AddRangeAsync(auditEntries, cancellationToken);
            }

            return await base.SavingChangesAsync(eventData, result, cancellationToken);
        }
    }



}
