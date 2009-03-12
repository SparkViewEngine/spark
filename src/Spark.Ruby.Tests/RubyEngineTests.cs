// Copyright 2008 Louis DeJardin - http://whereslou.com
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// 
using System.IO;
using Microsoft.Scripting;
using NUnit.Framework;

namespace Spark.Ruby.Tests
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

            var engine = IronRuby.Ruby.CreateEngine();
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