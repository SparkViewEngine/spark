using System.Xml.Linq;
using Spark;
using System.Xml.XPath ;

namespace Xpark
{
    public abstract class SparkView : AbstractSparkView
    {
        public XDocument Model { get; set; }

        void scratch()
        {
            
        }
        public object Eval(string expression)
        {
            return Model.XPathSelectElement(expression);
        }
        public string Eval(string expression, string format)
        {
            return string.Format(format, Model.XPathSelectElement(expression));
        }
    }
}