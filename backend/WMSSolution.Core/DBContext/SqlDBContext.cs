using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System.Data;
using System.Data.Common;
using System.Reflection;
using WMSSolution.Core.Models;
using WMSSolution.Core.Models.IntegrationWCS;

namespace WMSSolution.Core.DBContext;

/// <summary>
/// SqlDBContext
/// </summary>
/// <remarks>
/// dbcontext
/// </remarks>
/// <param name="options">options</param>
public class SqlDBContext(DbContextOptions<SqlDBContext> options) : DbContext(options)
{
    /// <summary>
    ///  current user's tenant_id
    /// </summary>
    public byte tenant_id { get; set; } = 1;

    /// <summary>
    /// Database
    /// </summary>
    /// <returns></returns>
    public DatabaseFacade GetDatabase() => Database;

    /// <summary>
    /// Gets or sets the collection of outbound entities in the database context.
    /// </summary>
    public DbSet<OutboundEntity> Outbounds { get; set; }
    /// <summary>
    /// Gets or sets the collection of inbound entities in the database context.
    /// </summary>
    /// <remarks>Use this property to query, add, update, or remove inbound records from the
    /// underlying database. Changes made to the collection are tracked by the context and persisted when
    /// SaveChanges is called.</remarks>
    public DbSet<InboundEntity> Inbounds { get; set; }

    /// <summary>
    /// Integration Historical
    /// </summary>
    public DbSet<IntegrationHistory> IntegrationHistories { get; set; }
    /// <summary>
    /// Swap Pallets
    /// </summary>
    public DbSet<SwapPalletEntity> SwapPallets { get; set; }

    /// <summary>
    /// Tenants
    /// </summary>
    public DbSet<TenantEntity> Tenants { get; set; }

    #region overwrite

    /// <summary>
    /// Auto Mapping Entity
    /// </summary>
    /// <param name="modelBuilder">ModelBuilder</param>
    private void MappingEntityTypes(ModelBuilder modelBuilder)
    {
        var baseType = typeof(BaseModel);
        var path = AppDomain.CurrentDomain.RelativeSearchPath ?? AppDomain.CurrentDomain.BaseDirectory;
        var referencedAssemblies = Directory.GetFiles(path, $"WMSSolution*.dll").Select(Assembly.LoadFrom).ToArray();
        var list = referencedAssemblies
            .SelectMany(a => a.DefinedTypes)
            .Select(type => type.AsType())
            .Where(x => x != baseType && baseType.IsAssignableFrom(x)).ToList();

        List<string> excludeMapping = ["BaseReceiptEntity", "BaseReceiptDetailEntity"];

        if (list != null && list.Count != 0)
        {
            list.ForEach(t =>
            {
                var entityType = modelBuilder.Model.FindEntityType(t);
                if (entityType == null && !excludeMapping.Contains(t.Name))
                {
                    modelBuilder.Model.AddEntityType(t);
                }
            });
        }
    }

    /// <summary>
    /// overwrite OnModelCreating
    /// </summary>
    /// <param name="modelBuilder"></param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<GlobalUniqueSerialEntity>();
        MappingEntityTypes(modelBuilder);
        /*foreach (var entityType in modelBuilder.Model.GetEntityTypes())
           {
               if (typeof(Models.IHasTenant).IsAssignableFrom(entityType.ClrType))
               {
                   ConfigureGlobalFiltersMethodInfo
                      .MakeGenericMethod(entityType.ClrType)
                      .Invoke(this, new object[] { modelBuilder });
               }
           }*/
        base.OnModelCreating(modelBuilder);
    }

    /// <summary>
    /// create DbSet
    /// </summary>
    /// <typeparam name="T">Entity</typeparam>
    /// <returns></returns>
    public virtual DbSet<T> GetDbSet<T>() where T : class
    {
        if (Model.FindEntityType(typeof(T)) != null)
        {
            return Set<T>();
        }
        else
        {
            throw new Exception($"type {typeof(T).Name} is not add into DbContext ");
        }
    }

    /// <summary>
    /// Returns a queryable collection of tenant-specific entities of type for the specified tenant.
    /// </summary>
    /// <remarks>When <paramref name="isTracking"/> is <see langword="false"/>, the returned query
    /// uses 'AsNoTracking', which improves performance for read-only operations but does not track changes to
    /// entities. Use tracking only when you intend to modify and save entities.</remarks>
    /// <typeparam name="T">The entity type to query. Must implement <see cref="ITenantEntity"/>.</typeparam>
    /// <param name="tenantId">The unique identifier of the tenant whose entities are to be retrieved.</param>
    /// <param name="isTracking">Indicates whether the returned query should enable change tracking. Specify <see langword="true"/> to track
    /// changes; otherwise, <see langword="false"/> to disable tracking.</param>
    /// <returns>An <see cref="IQueryable{T}"/> containing entities of type <typeparamref name="T"/> that belong to the
    /// specified tenant.</returns>
    public virtual IQueryable<T> GetDbSet<T>(long tenantId, bool isTracking = false) where T : class, ITenantEntity
    {
        return isTracking ? Set<T>().Where(e => e.TenantId == tenantId) :
            Set<T>().Where(e => e.TenantId == tenantId).AsNoTracking();
    }

    /// <summary>
    /// over write  EnsureCreated
    /// </summary>
    /// <returns></returns>
    public virtual bool EnsureCreated()
    {
        return Database.EnsureCreated();
    }

    /// <summary>
    /// over write OnConfiguring
    /// </summary>
    /// <param name="optionsBuilder"></param>
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
    }

    /// <summary>
    /// exec sql and get the first list
    /// </summary>
    /// <param name="sql">sql</param>
    /// <param name="parameters">parameters</param>
    /// <returns></returns>
    public virtual List<T> SqlQueryDynamic<T>(string sql, Dictionary<string, object> parameters) where T : new()
    {
        DbProviderFactory dbProvider = DbProviderFactories.GetFactory(GetDatabase().GetDbConnection());
        using (DbConnection connection = dbProvider.CreateConnection())
        using (DbDataAdapter adapter = dbProvider.CreateDataAdapter())
        using (DbCommand cmd = dbProvider.CreateCommand())
        {
            connection.ConnectionString = this.Database.GetConnectionString();
            if (connection.State != ConnectionState.Open)
            {
                connection.Open();
            }
            cmd.CommandTimeout = 600;
            cmd.Connection = connection;
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = sql;
            adapter.SelectCommand = cmd;

            if (parameters != null)
            {
                foreach (var item in parameters)
                {
                    DbParameter dbParameter = cmd.CreateParameter();
                    dbParameter.ParameterName = item.Key;
                    dbParameter.Value = item.Value;
                    dbParameter.Direction = ParameterDirection.Input;
                    cmd.Parameters.Add(dbParameter);
                }
            }

            DataTable dataTable = new("Table");
            adapter.Fill(dataTable);
            if (dataTable.Rows.Count > 0)
            {
                return DataTableToIList<T>(dataTable);
            }
            else
            {
                return [];//default(List<T>);
            }
        }
    }

    /// <summary>
    /// convert DataTable type data to List type data
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="dt"></param>
    /// <returns></returns>
    public static List<T> DataTableToIList<T>(DataTable dt)
    {
        if (dt == null) return [];

        DataTable property_data = dt;
        List<T> result = [];
        for (int j = 0; j < property_data.Rows.Count; j++)
        {
            T new_data = (T)Activator.CreateInstance(typeof(T));
            PropertyInfo[] propertys = new_data.GetType().GetProperties();
            foreach (PropertyInfo pi in propertys)
            {
                for (int i = 0; i < property_data.Columns.Count; i++)
                {
                    if (pi.Name.Equals(property_data.Columns[i].ColumnName))
                    {
                        if (property_data.Rows[j][i] != DBNull.Value)
                        {
                            //special handling
                            if (pi.PropertyType.FullName == "System.Boolean")
                            {
                                var val = property_data.Rows[j][i].ToString() == "1";
                                pi.SetValue(new_data, val, null);
                            }
                            else
                            {
                                pi.SetValue(new_data, property_data.Rows[j][i], null);
                            }
                        }
                        else
                        {
                            pi.SetValue(new_data, null, null);
                        }

                        break;
                    }
                }
            }
            result.Add(new_data);
        }

        return result;
    }

    #endregion overwrite
}