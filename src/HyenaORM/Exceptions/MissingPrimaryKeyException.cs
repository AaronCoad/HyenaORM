using System;

namespace HyenaORM.Exceptions
{
    public class MissingPrimaryKeyException : Exception
    {
        private static string _message = @"{0} is missing a primary key.";
        internal MissingPrimaryKeyException(Type type)
        : base(string.Format(_message, type.Name))
        { }
    }
}