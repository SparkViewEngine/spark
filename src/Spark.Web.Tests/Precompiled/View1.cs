// Copyright 2008-2024 Louis DeJardin
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
using System;
using Spark.Tests.Stubs;

namespace Spark.Tests.Precompiled
{
    [SparkView(
        TargetNamespace = "Spark.Tests.Precompiled", 
        Templates = new[] { "Foo\\Bar.spark", "Shared\\Quux.spark" })]
    public class View1 : StubSparkView
    {
        public override void Render()
        {
            Output.Write("<p>Hello world</p>");
        }

        public override Guid GeneratedViewId => new Guid("11111111123412341234123456123456");

        public override bool TryGetViewData(string name, out object value)
        {
            throw new NotImplementedException();
        }
    }
}
