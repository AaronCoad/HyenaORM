using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace HyenaORM.Interfaces
{
    public interface IDatabase
    {
        string ConnectionString { get; }

        Task<IEnumerable<T>> LoadAllRecords<T>();
        Task<T> LoadRecordByPrimaryKey<T>(object primaryKey);
    }
}
