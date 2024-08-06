using System;

namespace HyenaORM.Exceptions
{
    public class MissingTableNameException : Exception
    {
        private static string _message = @"{0} is missing the TableName attribute.";
        internal MissingTableNameException(Type type)
        : base(string.Format(_message, type.Name))
        { }
    }
}