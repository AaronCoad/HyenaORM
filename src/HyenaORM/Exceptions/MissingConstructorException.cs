using System;

namespace HyenaORM.Exceptions
{
    public class MissingConstructorException : Exception
    {
        private static string _message = @"{0} is missing a parameterless constructor.";
        internal MissingConstructorException(Type type)
        : base(string.Format(_message, type.Name))
        { }
    }
}