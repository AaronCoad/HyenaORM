using System;
using System.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Collections;
using System.Reflection;
using System.Linq;
using System.Data;
using HyenaORM.Attributes;

namespace HyenaORM
{
    // Currently only supports a single database.
    // Should eventually have the option of supporting multiple databases in one project.
    public static class Database
    {
        private static string ConnectionString { get; set; }

        // Initialise the connection string
        public static void Init(string connectionString)
        {
            ConnectionString = connectionString;
        }

        // Return an IEnumerable of the type based on data.
        // This method uses reflection to identify the fields we are interested in and the appropriate table.
        public static IEnumerable<T> GetAll<T>()
        {
            List<T> results = new List<T>();
            Type type = typeof(T);
            List<string> fieldNames = new List<string>();

            // Throw an exception if the class is missing a table name.
            if (type.GetCustomAttribute<TableNameAttribute>() == null)
            {
                throw new Exception(String.Format("Class {0} is missing the TableNameAttribute", type.Name));
            }

            // Get all the properties with a field name.
            PropertyInfo[] propertyInfos = type.GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(x => x.GetCustomAttribute<FieldNameAttribute>() != null).ToArray();

            // Throw an exception if there are no properties with a field name.
            if (propertyInfos.Count() == 0)
            {
                throw new Exception(String.Format("Class {0} has no properties with the FieldNameAttribute", type.Name));
            }

            // Get the list of field names for our select.
            foreach(var prop in propertyInfos)
            {
                fieldNames.Add(prop.GetCustomAttribute<FieldNameAttribute>().Name);
            }

            using(SqlCommand cmd = new SqlConnection(ConnectionString).CreateCommand())
            {
                cmd.CommandText = String.Format("Select {0} from {1}", String.Join(",",fieldNames.ToArray()), type.GetCustomAttribute<TableNameAttribute>().Name);
                cmd.Connection.Open();

                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    if (reader.HasRows)
                    {
                        T item = (T)typeof(T).GetConstructor(Type.EmptyTypes).Invoke(null);

                        foreach(var prop in propertyInfos)
                        {
                            int pos = reader.GetOrdinal(prop.GetCustomAttribute<FieldNameAttribute>().Name);

                            // If the returned column isn't null then we set the value.
                            if (!reader.IsDBNull(pos))
                            {
                                prop.SetValue(item, reader.GetValue(pos));
                            }
                        }

                        results.Add(item);
                    }
                }
                cmd.Connection.Close();
            }
            return results;
        }

        public static T GetRecordByPrimaryKey<T>(object primaryKeyValue)
        {
            T result = (T)typeof(T).GetConstructor(Type.EmptyTypes).Invoke(null);
            Type type = typeof(T);
            List<string> fieldNames = new List<string>();
            
            // Throw an exception if the class is missing a table name.
            if (type.GetCustomAttribute<TableNameAttribute>() == null)
            {
                throw new Exception(String.Format("Class {0} is missing the TableNameAttribute", type.Name));
            }

            // Get all the properties with a field name.
            PropertyInfo[] propertyInfos = type.GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(x => x.GetCustomAttribute<FieldNameAttribute>() != null).ToArray();

            // Throw an exception if there are no properties with a field name.
            if (propertyInfos.Count() == 0)
            {
                throw new Exception(String.Format("Class {0} has no properties with the FieldNameAttribute", type.Name));
            }

            // Get the property containing the primary key attribute
            // and then get the name from the field name attribute.
            string primaryKeyName = "";

            // Get the list of field names for our select.
            foreach (var prop in propertyInfos)
            {
                fieldNames.Add(prop.GetCustomAttribute<FieldNameAttribute>().Name);
                primaryKeyName = prop.GetCustomAttribute<PrimaryKeyAttribute>() != null ? prop.GetCustomAttribute<FieldNameAttribute>().Name : "";
            }

            if (String.IsNullOrWhiteSpace(primaryKeyName))
            {
                throw new Exception(String.Format("Class {0} is missing a primary key", type.Name));
            }

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

                cmd.Connection.Open();

                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    if (reader.HasRows)
                    {
                        foreach (var prop in propertyInfos)
                        {
                            int pos = reader.GetOrdinal(prop.GetCustomAttribute<FieldNameAttribute>().Name);

                            // If the returned column isn't null then we set the value.
                            if (!reader.IsDBNull(pos))
                            {
                                prop.SetValue(result, reader.GetValue(pos));
                            }
                        }
                    }
                }
                cmd.Connection.Close();
            }
            return result;
        }
    }
}
