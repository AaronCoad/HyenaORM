using System;
using System.Linq;
using System.Reflection;
using HyenaORM.Attributes;

namespace HyenaORM
{
    internal static class TypeHelpers
    {
        private static bool CheckFieldNameExpression(PropertyInfo x) => x.GetCustomAttribute<FieldNameAttribute>() != null;
        private static BindingFlags _bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
        internal static PropertyInfo[] GetFieldNamesFromProperties(this Type type) => type.GetProperties(_bindingFlags).Where(CheckFieldNameExpression).ToArray();
        internal static string GetTableNameForType(this Type type) => type.GetCustomAttribute<TableNameAttribute>() != null ? type.GetCustomAttribute<TableNameAttribute>().Name : "";
    }

    internal static class PropertyInfoHelpers
    {
        internal static string GetPrimaryKeyFieldName(this PropertyInfo[] propertyInfos) => propertyInfos.Where(x => x.GetCustomAttribute<PrimaryKeyAttribute>() != null).Select(x => x.GetCustomAttribute<FieldNameAttribute>().Name).First();
    }
}