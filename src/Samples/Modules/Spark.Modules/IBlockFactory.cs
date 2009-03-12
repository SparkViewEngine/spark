using System.Web.Mvc;

namespace Spark.Modules
{
    public interface IBlockFactory
    {
        IBlock CreateBlock(string blockName);
        void ReleaseBlock(IBlock block);
    }
}
