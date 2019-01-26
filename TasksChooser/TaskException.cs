using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace Amporis.TasksChooser
{
    public class TaskException : Exception
    {
        public TaskException() : base() { }

        public TaskException(string message) : base(message) { }

        [SecuritySafeCritical]
        protected TaskException(SerializationInfo info, StreamingContext context) : base(info, context) { }

        public TaskException(string message, Exception innerException) : base(message, innerException) { }


        public string InternalMessage { get; set; }

        public object Tag { get; set; }

        public static TaskException Clone(TaskException source, string newMessage)
        {
            return new TaskException(newMessage)
            {
                HelpLink = source.HelpLink,
                HResult = source.HResult,
                InternalMessage = source.Message + " " + Environment.NewLine + source.InternalMessage,
                Source = source.Source,
                Tag = source.Tag,
            };
        }


    }
}
