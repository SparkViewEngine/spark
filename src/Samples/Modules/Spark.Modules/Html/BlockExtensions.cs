using System;
using System.Web.Mvc;

namespace Spark.Modules.Html
{
    public static class BlockExtensions
    {
        public static void RenderBlock(this HtmlHelper helper, string blockName)
        {
            var controllerFactory = ControllerBuilder.Current.GetControllerFactory();
            var blockFactory = controllerFactory as IBlockFactory;
            if (blockFactory == null)
                throw new ApplicationException("IBlockFactory not available");

            var block = blockFactory.CreateBlock(helper.ViewContext, blockName);
            block.RenderBlock();
            blockFactory.ReleaseBlock(block);
        }
    }
}
