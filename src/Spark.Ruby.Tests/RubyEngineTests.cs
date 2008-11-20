using System.IO;
using IronRuby;
using Microsoft.Scripting;
using NUnit.Framework;

namespace Spark.IronRuby.Tests
{
    [TestFixture]
    public class RubyEngineTests
    {
        [Test]
        public void RunScriptInEngine()
        {
            var code = @"
class<<view
 def render
  write 'hello,'
  output.write ' world'
 end 
end

view.render

";
            var output = new StringWriter();

            var engine = Ruby.CreateEngine();
            var source = engine.CreateScriptSourceFromString(code, SourceCodeKind.File);
            var compiled = source.Compile();

            var scope = compiled.Engine.CreateScope();
            scope.SetVariable("view", new View1234 { Output = output });
            compiled.Execute(scope);

            Assert.AreEqual("hello, world", output.ToString());
        }
    }

    public class View1234
    {
        public TextWriter Output { get; set; }
        public void Write(object x)
        {
            Output.Write(x);
        }
    }
}
