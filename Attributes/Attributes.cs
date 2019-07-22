using System;
using System.Collections.Generic;
using System.Text;

namespace HyenaORM.Attributes
{
    [System.AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class TableNameAttribute : Attribute
    {
        public string Name { get; set; }

        public TableNameAttribute(string name)
        {
            this.Name = name;
        }
    }

    [System.AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public sealed class FieldNameAttribute : Attribute
    {
        public string Name { get; set; }

        public FieldNameAttribute(string name)
        {
            this.Name = name;
        }
    }

    [System.AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public sealed class PrimaryKeyAttribute : Attribute
    {
        
    }
}
