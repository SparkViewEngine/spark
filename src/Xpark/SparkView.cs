using System.Xml.Linq;
using Spark;
using System.Xml.XPath ;

namespace Xpark
{
    public abstract class SparkView : AbstractSparkView
    {
        public XDocument Model { get; set; }
    }
}