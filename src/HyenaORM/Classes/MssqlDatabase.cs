using System.Collections.Generic;
using System.Threading.Tasks;
using HyenaORM.Classes;
using Microsoft.Extensions.Configuration;

namespace HyenaORM
{
    // Currently only supports a single database.
    // Should eventually have the option of supporting multiple databases in one project.
    public class MssqlDatabase : BaseDatabase
    {
        public MssqlDatabase(string connectionString) : base(connectionString)
        {
            base._parameterPrefix = "@";
        }

        public MssqlDatabase(IConfiguration configuration) : base(configuration)
        {
            base._parameterPrefix = "@";
        }

        public MssqlDatabase(IConfigurationSection configurationSection) : base(configurationSection)
        {
            base._parameterPrefix = "@";
        }

        // Return an IEnumerable of the type.
        public new async Task<IEnumerable<T>> LoadAllRecords<T>()
        {
            return await base.LoadAllRecords<T>();
        }

        // Return a single record of the type.
        public new async Task<T> LoadRecordByPrimaryKey<T>(object primaryKey)
        {
            return await base.LoadRecordByPrimaryKey<T>(primaryKey);
        }
    }
}
