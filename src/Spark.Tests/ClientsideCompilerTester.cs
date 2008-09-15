using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Spark.FileSystem;

namespace Spark.Tests
{
    [TestFixture]
    public class ClientsideCompilerTester
    {
        [Test]
        public void GenerateSimpleTemplate()
        {
            var descriptor = new SparkViewDescriptor()
                .SetLanguage(LanguageType.Javascript)
                .AddTemplate("Clientside\\simple.spark");

            var engine = new SparkViewEngine { ViewFolder = new FileSystemViewFolder("Spark.Tests.Views") };
            var entry = engine.CreateEntry(descriptor);

            Assert.IsNotNull(entry.SourceCode);
            Assert.IsNotEmpty(entry.SourceCode);
        }
    }
}
