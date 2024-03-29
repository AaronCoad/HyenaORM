﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;
using HyenaORM.Attributes;
using System.Linq;

namespace HyenaORM.Classes
{
    public class BaseDatabase
    {
        #region Fields and Properties
        private string _connectionString = "";
        private BindingFlags _bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
        protected string _missingFieldNamesException = @"{0} has no properties with the FieldName attribute.";
        protected string _missingTableNameException = @"{0} is missing the TableName attribute.";
        protected string _missingConstructorException = @"{0} is missing a parameterless constructor.";
        protected string _missingPrimaryKeyException = @"{0} is missing a primary key.";
        protected string _parameterPrefix;
        #endregion

        #region Constructors
        public BaseDatabase(string connectionString) => SetConnectionString(connectionString);
        public BaseDatabase(IConfiguration configuration) => SetConnectionString(ConnectionStringFromConfigurationSection(configuration.GetSection("HyenaORM")));
        public BaseDatabase(IConfigurationSection configurationSection) => SetConnectionString(ConnectionStringFromConfigurationSection(configurationSection));
        #endregion

        #region Methods
        private bool CheckFieldNameExpression(PropertyInfo x) => x.GetCustomAttribute<FieldNameAttribute>() != null;
        private string ConnectionStringFromConfigurationSection(IConfigurationSection section) => 
            new SqlConnectionStringBuilder 
            {
                DataSource = section.GetValue<string>("Server"),
                InitialCatalog = section.GetValue<string>("Database"),
                IntegratedSecurity = section.GetValue("OSAuth", false),
                UserID = section.GetValue<string>("Username"),
                Password = section.GetValue<string>("Password"),
                MultipleActiveResultSets = section.GetValue("MultipleResultSets", true)
            }.ToString();
        protected string GetPrimaryKeyFieldName(PropertyInfo[] propertyInfos)
        {
            foreach (var prop in propertyInfos)
            {
                if (prop.GetCustomAttribute<PrimaryKeyAttribute>() != null)
                {
                    return prop.GetCustomAttribute<FieldNameAttribute>().Name;
                }
            }

            return "";
        }
        protected PropertyInfo[] GetFieldNamesFromProperties(Type type) => type.GetProperties(_bindingFlags).Where(CheckFieldNameExpression).ToArray();
        protected string GetTableNameFromProperties(Type type)
        {
            return type != null && type.GetCustomAttribute<TableNameAttribute>() != null
                ? type.GetCustomAttribute<TableNameAttribute>().Name
                : "";
        }
        private void SetConnectionString(string connectionString) => _connectionString = connectionString;
        private string GetCommandText(PropertyInfo[] propertyInfos, string tableName) => 
            string.Format("Select {0} from {1}",
                String.Join(",", propertyInfos.Select(x => x.GetCustomAttribute<FieldNameAttribute>().Name).ToArray()),
                tableName);
        private T CreateObjectFromReader<T>(SqlDataReader reader, PropertyInfo[] propertyInfos, ConstructorInfo constructor)
        {
            T item = (T)constructor.Invoke(null);
            foreach (var prop in propertyInfos)
            {
                object value = reader[prop.GetCustomAttribute<FieldNameAttribute>().Name];
                if (value != DBNull.Value)
                {
                    prop.SetValue(item, value);
                }
            }

            return item;
        }
        #endregion

        #region Inherited Methods
        // Return an IEnumerable of the type.
        public async Task<IEnumerable<T>> LoadAllRecords<T>()
        {
            Type type = typeof(T);
            PropertyInfo[] propertyInfos = GetFieldNamesFromProperties(type);
            string tableName = GetTableNameFromProperties(type);
            ConstructorInfo constructorInfo = type.GetConstructor(Type.EmptyTypes);

            if (propertyInfos != null && !propertyInfos.Any())
                throw new Exception(string.Format(_missingFieldNamesException, type.Name));

            if (string.IsNullOrWhiteSpace(tableName))
                throw new Exception(string.Format(_missingTableNameException, type.Name));

            if (constructorInfo == null)
                throw new Exception(string.Format(_missingConstructorException, type.Name));

            List<T> results = new List<T>();

            using (SqlCommand cmd = new SqlConnection(_connectionString).CreateCommand())
            {
                cmd.CommandText = GetCommandText(propertyInfos, tableName);

                try
                {
                    await cmd.Connection.OpenAsync();

                    SqlDataReader reader = await cmd.ExecuteReaderAsync(CommandBehavior.SequentialAccess);

                    while (reader.Read())
                    {
                        if (reader.HasRows)
                            results.Add(CreateObjectFromReader<T>(reader,propertyInfos,constructorInfo));
                    }
                    cmd.Connection.Close();
                }
                catch (Exception ex)
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
        public async Task<T> LoadRecordByPrimaryKey<T>(object primaryKey)
        {
            T result;
            Type type = typeof(T);
            PropertyInfo[] propertyInfos = GetFieldNamesFromProperties(type);
            string tableName = GetTableNameFromProperties(type);
            string primaryKeyName = GetPrimaryKeyFieldName(propertyInfos);
            ConstructorInfo constructorInfo = type.GetConstructor(Type.EmptyTypes);

            if (propertyInfos != null && !propertyInfos.Any())
                throw new Exception(string.Format(_missingFieldNamesException, type.Name));

            if (string.IsNullOrWhiteSpace(primaryKeyName))
                throw new Exception(string.Format(_missingPrimaryKeyException, type.Name));

            if (string.IsNullOrWhiteSpace(tableName))
                throw new Exception(string.Format(_missingTableNameException, type.Name));

            if (constructorInfo == null)
                throw new Exception(string.Format(_missingConstructorException, type.Name));

            result = (T)constructorInfo.Invoke(parameters: null);

            using (SqlCommand cmd = new SqlConnection(connectionString: _connectionString).CreateCommand())
            {
                cmd.CommandText = String.Format(format: "{0} where {1} = @{1}",
                                                arg0: GetCommandText(propertyInfos,tableName),
                                                arg1: primaryKeyName);

                cmd.Parameters.Add(value: new SqlParameter
                {
                    ParameterName = String.Format(format: "@{0}", arg0: primaryKeyName),
                    Value = primaryKey
                });

                try
                {
                    await cmd.Connection.OpenAsync();

                    SqlDataReader reader = await cmd.ExecuteReaderAsync(CommandBehavior.SequentialAccess);

                    while (reader.Read())
                    {
                        if (reader.HasRows)
                            result = CreateObjectFromReader<T>(reader, propertyInfos, constructorInfo);
                    }
                }
                catch (Exception ex)
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
        #endregion
    }
}
