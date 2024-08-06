using System;

namespace HyenaORM.Exceptions
{
    public class MissingFieldNamesException : Exception
    {
        private static string _message = @"{0} has no properties with the FieldName attribute.";
        internal MissingFieldNamesException(Type type)
        : base(string.Format(_message, type.Name))
        { }
    }
}