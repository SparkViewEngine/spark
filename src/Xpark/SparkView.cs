using System.Xml.Linq;
using Spark;

namespace Xpark
{
    public abstract class SparkView : AbstractSparkView
    {
        public XDocument Model { get; set; }
    }
}