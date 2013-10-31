using System;

namespace OptimaJet.Common
{
    public class NotValidationException : Exception
    {
        public NotValidationException(string msg)
            : base(msg)
        {
        }
    }

    public class ObjectNotFoundException : Exception
    {
        public ObjectNotFoundException(string msg)
            : base(msg)
        {
        }
    }
}