using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Spark.Spool;

namespace Spark
{
    public interface ISparkViewDecorator : ISparkView
    {
        ISparkView Decorated { get; }
    }

    public abstract class SparkViewDecorator<TExtendedContext> : SparkViewBase<TExtendedContext>, ISparkViewDecorator
    {
        private readonly SparkViewBase<TExtendedContext> _decorated;

        protected SparkViewDecorator(SparkViewBase<TExtendedContext> decorated)
        {
            _decorated = decorated;
        }

        ISparkView ISparkViewDecorator.Decorated
        {
            get { return _decorated; }
        }

        public override SparkViewContext<TExtendedContext> SparkViewContext
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
                var spooled = new SpoolWriter();
                _decorated.RenderView(spooled);
                Content["view"] = spooled;
            }
            base.RenderView(writer);
        }

    }
}
