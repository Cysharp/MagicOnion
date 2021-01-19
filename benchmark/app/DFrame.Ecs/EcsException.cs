using System;
using System.Collections.Generic;
using System.Text;

namespace DFrame.Ecs
{
    public class EcsException : Exception
    {
        public EcsException()
        {
        }
        public EcsException(string message)
            : base(message)
        {
        }
        public EcsException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
