using System;
using System.Runtime.Serialization;

namespace DemoControls.SubdivisionStrategies
{
    [Serializable]
    public class SubdivisionStrategyException : Exception
    {
        public SubdivisionStrategyException()
        {
        }

        public SubdivisionStrategyException(string message) : base(message)
        {
        }

        public SubdivisionStrategyException(string message, Exception inner) : base(message, inner)
        {
        }

        protected SubdivisionStrategyException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}