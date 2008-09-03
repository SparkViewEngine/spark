using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MvcContrib.ViewFactories;
using Spark;

namespace MvcContrib.SparkViewEngine
{
    public class SparkViewFactory : Spark.Web.Mvc.SparkViewFactory
    {
        public SparkViewFactory()
        {

        }

        public SparkViewFactory(ISparkSettings settings)
            : base(settings)
        {

        }

        public IViewSourceLoader ViewSourceLoader
        {
            get
            {
                if (ViewFolder is ViewSourceLoaderWrapper)
                    return ((ViewSourceLoaderWrapper)ViewFolder).ViewSourceLoader;

                return null;
            }
            set
            {
                ViewFolder = new ViewSourceLoaderWrapper(value);
            }
        }
    }
}
