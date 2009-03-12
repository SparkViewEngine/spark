using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web;

namespace EmailOrTextTemplating.Services
{
    public abstract class MessageBuilder
    {
        private static MessageBuilder _instance;

        public static MessageBuilder Current
        {
            get
            {
                return _instance ?? 
                    Interlocked.CompareExchange(ref _instance, new DefaultMessageBuilder(), null) ?? 
                    _instance;
            }
            set { _instance = value; }
        }

        public abstract void Transform(string message, object data, TextWriter output);

        public string Transform(string message, object data)
        {
            var writer = new StringWriter();
            Transform(message, data, writer);
            return writer.ToString();
        }
    }
}
