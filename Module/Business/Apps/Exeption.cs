using System;
using System.Runtime.Serialization;

namespace VMP_CNR.Module.Business.Apps
{
    [Serializable]
    internal class Exeption : Exception
    {
        public Exeption()
        {
        }

        public Exeption(string message) : base(message)
        {
        }

        public Exeption(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected Exeption(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}