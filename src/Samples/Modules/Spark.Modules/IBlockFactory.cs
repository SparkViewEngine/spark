using System.Web.Mvc;

namespace Spark.Modules
{
    public interface IBlockFactory
    {
        IBlock CreateBlock(ViewContext viewContext, string blockName);
        void ReleaseBlock(IBlock block);
    }
}
