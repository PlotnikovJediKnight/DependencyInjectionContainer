using System;
using System.Runtime.Serialization;

namespace DependencyInjectionContainer.CustomExceptions
{
    [Serializable]
    public class DependencyException : System.Exception
    {
        public DependencyException() { }

        protected DependencyException(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        public DependencyException(string message) 
            : base(message) { }

        public DependencyException(string message, System.Exception innerException) 
            : base(message, innerException) { }
    }
}
