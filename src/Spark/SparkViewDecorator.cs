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

    public abstract class SparkViewDecorator<TExtended> : SparkViewBase<TExtended>, ISparkViewDecorator where TExtended : new()
    {
        private readonly SparkViewBase<TExtended> _decorated;

        protected SparkViewDecorator(SparkViewBase<TExtended> decorated)
        {
            _decorated = decorated;
        }

        ISparkView ISparkViewDecorator.Decorated
        {
            get { return _decorated; }
        }

        public override SparkContext<TExtended> SparkContext
        {
            get
            {
                return _decorated != null ? _decorated.SparkContext : base.SparkContext;
            }
            set
            {
                if (_decorated != null)
                    _decorated.SparkContext = value;
                else
                    base.SparkContext = value;
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
