using System.IO;

namespace Spark.Web.Mvc.Wrappers {
    public interface ITextWriterContainer {
        TextWriter Output{ get; set;}
    }
}
