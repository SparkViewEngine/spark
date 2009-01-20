using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spark
{
    public abstract class SparkViewDecorator : SparkViewBase
    {
        private readonly SparkViewBase _decorated;

        protected SparkViewDecorator(SparkViewBase decorated)
        {
            _decorated = decorated;
        }

        public override SparkViewContext SparkViewContext
        {
            get
            {
                return _decorated != null ? _decorated.SparkViewContext : base.SparkViewContext;
            }
            set
            {
                if (_decorated != null)
                    _decorated.SparkViewContext = value;
                else
                    base.SparkViewContext = value;
            }
        }

        public override void RenderView(System.IO.TextWriter writer)
        {
            if (_decorated != null)
            {
                using (OutputScope())
                {
                    _decorated.RenderView(Output);
                    Content["view"] = Output;
                }
            }            
        }
    }
}
