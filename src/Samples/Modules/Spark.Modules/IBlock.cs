using System.Web.Mvc;

namespace Spark.Modules
{
    public interface IBlock
    {
        void RenderBlock(ViewContext viewContext);
    }
}