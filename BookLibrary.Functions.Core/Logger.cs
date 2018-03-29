using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookLibrary.Functions
{
    public class Logger
    {
        private ILog log;
        public Logger(string name)
        {
            log = LogManager.GetLogger(name);
        }

        public Logger(Type type)
        {
            log = LogManager.GetLogger(type);
        }

        public Logger(string name, Type type)
        {
            log = LogManager.GetLogger(name, type);
        }

        public Exception Error(object message)
        {
            Exception ex = new Exception(message.ToString());
            return Error(message, ex);
        }

        public Exception Error(object message, Exception ex)
        {
            log.Error(message, ex);
            return ex;
        }

        public bool Warning(object message)
        {
            log.Warn(message);
            return true;
        }

        public bool Info(object message)
        {
            log.Info(message);
            return true;
        }
    }
}
