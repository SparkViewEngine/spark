using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Web.Mvc;

namespace Spark.Modules
{
    public class BlockBuilder
    {
        private static BlockBuilder _current;

        public static BlockBuilder Current
        {
            get
            {
                return _current ?? Interlocked.CompareExchange(ref _current, new BlockBuilder(), null) ?? _current;
            }
        }

        public IBlockFactory GetBlockFactory()
        {
            return ControllerBuilder.Current.GetControllerFactory() as IBlockFactory;
        }
    }
}
