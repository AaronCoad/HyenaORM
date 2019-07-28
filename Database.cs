using System;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Collections;
using System.Reflection;
using System.Linq;
using System.Data;
using System.Threading.Tasks;
using HyenaORM.Attributes;

namespace HyenaORM
{
    // Currently only supports a single database.
    // Should eventually have the option of supporting multiple databases in one project.
    public static class Database
    {
        private static string ConnectionString { get; set; }

        // Get the Primary Key field name
        private static string GetPrimaryKeyFieldName(PropertyInfo[] propertyInfos)
        {
            foreach(var prop in propertyInfos)
            {
                if(prop.GetCustomAttribute<PrimaryKeyAttribute>() != null)
                {
                    return prop.GetCustomAttribute<FieldNameAttribute>().Name;
                }
            }

            return "";
        }

        // Validate whether the appropriate attributes are assigned
        private static bool GetValidationErrors(Type type, bool includePKCheck, out string exceptionMessage)
        {
            exceptionMessage = "";
            PropertyInfo[] propertyInfos = type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).Where(x => x.GetCustomAttribute<FieldNameAttribute>() != null).ToArray();

            // Check if the Type has at least 1 property with the FieldName attribute
            if (propertyInfos.Count() == 0)
                exceptionMessage = String.Format("{0} has no properties with the FieldName attribute", type.Name);

            // Check if the Type has a property with the PrimaryKey attribute
            // Only apply this check if we are using the LoadRecordByPrimaryKey
            if (String.IsNullOrWhiteSpace(GetPrimaryKeyFieldName(propertyInfos)) && includePKCheck)
            {
                if (String.IsNullOrWhiteSpace(exceptionMessage))
                    exceptionMessage = String.Format("{0} has no properties with the PrimaryKey attribute", type.Name);
                else
                    exceptionMessage = String.Format("{0};\r\n{1} has no properties with the PrimaryKey attribute", exceptionMessage, type.Name);
            }

            // Check if the Type has the TableName attribute linked to the Type
            if (type.GetCustomAttribute<TableNameAttribute>() == null)
            {
                if (String.IsNullOrWhiteSpace(exceptionMessage))
                    exceptionMessage = String.Format("{0} has no properties with the TableName attribute", type.Name);
                else
                    exceptionMessage = String.Format("{0};\r\n{1} has no properties with the TableName attribute", exceptionMessage, type.Name);
            }

            return !String.IsNullOrWhiteSpace(exceptionMessage);
        }

        // Initialise the connection string
        public static void Init(string connectionString)
        {
            ConnectionString = connectionString;
        }

        // Return an IEnumerable of the type.
        public static async Task<IEnumerable<T>> GetAll<T>()
        {
            string message = "";

            // Run validation check before anything else
            if (GetValidationErrors(typeof(T), false, out message))
                throw new Exception(message);

            List<T> results = new List<T>();
            Type type = typeof(T);
            List<string> fieldNames = new List<string>();

            // Get all the properties with a field name.
            PropertyInfo[] propertyInfos = type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).Where(x => x.GetCustomAttribute<FieldNameAttribute>() != null).ToArray();

            // Get the list of field names for our select.
            foreach (var prop in propertyInfos)
                fieldNames.Add(prop.GetCustomAttribute<FieldNameAttribute>().Name);

            using (SqlCommand cmd = new SqlConnection(ConnectionString).CreateCommand())
            {
                cmd.CommandText = String.Format("Select {0} from {1}",
                                                String.Join(",",fieldNames.ToArray()),
                                                type.GetCustomAttribute<TableNameAttribute>().Name);

                try
                {
                    await cmd.Connection.OpenAsync();

                    SqlDataReader reader = await cmd.ExecuteReaderAsync();

                    while (reader.Read())
                    {
                        if (reader.HasRows)
                        {
                            T item = (T)typeof(T).GetConstructor(Type.EmptyTypes).Invoke(null);

                            foreach (var prop in propertyInfos)
                            {
                                int pos = reader.GetOrdinal(prop.GetCustomAttribute<FieldNameAttribute>().Name);
                                object value = reader.GetValue(pos);
                                
                                // If the returned column isn't null then we set the value.
                                if (value != DBNull.Value)
                                {
                                    prop.SetValue(item, value);
                                }
                            }

                            results.Add(item);
                        }
                    }
                    cmd.Connection.Close();
                }
                catch(Exception ex)
                {
                    throw ex;
                }
                finally
                {
                    if (cmd.Connection.State == ConnectionState.Open)
                        cmd.Connection.Close();
                }
            }
            return results;
        }

        // Return a single record of the type.
        public static async Task<T> GetRecordByPrimaryKey<T>(object primaryKeyValue)
        {
            string message = "";

            // Run validation check before anything else
            if (GetValidationErrors(typeof(T), true, out message))
                throw new Exception(message);

            T result = (T)typeof(T).GetConstructor(Type.EmptyTypes).Invoke(null);
            Type type = typeof(T);
            List<string> fieldNames = new List<string>();

            // Get all the properties with a field name.
            PropertyInfo[] propertyInfos = type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).Where(x => x.GetCustomAttribute<FieldNameAttribute>() != null).ToArray();

            // Get the list of field names for our select.
            foreach (var prop in propertyInfos)
                fieldNames.Add(prop.GetCustomAttribute<FieldNameAttribute>().Name);

            // Get the primary key name
            string primaryKeyName = GetPrimaryKeyFieldName(propertyInfos);

            using (SqlCommand cmd = new SqlConnection(ConnectionString).CreateCommand())
            {
                cmd.CommandText = String.Format("Select {0} from {1} where {2} = @{2}", 
                                                String.Join(",", fieldNames.ToArray()), 
                                                type.GetCustomAttribute<TableNameAttribute>().Name,
                                                primaryKeyName);

                cmd.Parameters.Add(new SqlParameter()
                {
                    ParameterName = String.Format("@{0}", primaryKeyName),
                    Value = primaryKeyValue
                });

                try
                {
                    await cmd.Connection.OpenAsync();

                    SqlDataReader reader = await cmd.ExecuteReaderAsync();

                    while (reader.Read())
                    {
                        if (reader.HasRows)
                        {
                            foreach (var prop in propertyInfos)
                            {
                                int pos = reader.GetOrdinal(prop.GetCustomAttribute<FieldNameAttribute>().Name);
                                object value = reader.GetValue(pos);

                                // If the returned column isn't null then we set the value.
                                if (value != DBNull.Value)
                                {
                                    prop.SetValue(result, value);
                                }
                            }
                        }
                    }
                }
                catch(Exception ex)
                {
                    throw ex;
                }
                finally
                {
                    if (cmd.Connection.State == ConnectionState.Open)
                        cmd.Connection.Close();
                }
            }
            return result;
        }
    }
}
