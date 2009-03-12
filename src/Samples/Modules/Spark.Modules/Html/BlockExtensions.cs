using System;
using System.Web.Mvc;

namespace Spark.Modules.Html
{
    public static class BlockExtensions
    {
        public static void RenderBlock(this HtmlHelper helper, string blockName)
        {
            RenderBlockImplementation(blockName, helper.ViewContext, helper.ViewData);
        }

        public static void RenderBlock(this HtmlHelper helper, string blockName, object model)
        {
            RenderBlockImplementation(blockName, helper.ViewContext, new ViewDataDictionary(model));
        }

        public static void RenderBlock(this HtmlHelper helper, string blockName, ViewDataDictionary viewData)
        {
            RenderBlockImplementation(blockName, helper.ViewContext, viewData);
        }

        public static void RenderBlock(this HtmlHelper helper, string blockName, object model, ViewDataDictionary viewData)
        {
            RenderBlockImplementation(blockName, helper.ViewContext, new ViewDataDictionary(viewData) { Model = model });
        }

        private static void RenderBlockImplementation(string blockName, ViewContext viewContext, ViewDataDictionary viewData)
        {
            var blockFactory = BlockBuilder.Current.GetBlockFactory();
            if (blockFactory == null)
                throw new ApplicationException("IBlockFactory not available from current controller factory");

            var block = blockFactory.CreateBlock(blockName);
            block.RenderBlock(viewContext);
            blockFactory.ReleaseBlock(block);
        }
    }
}
