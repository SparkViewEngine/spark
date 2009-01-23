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
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Spark.Tests.Stubs;

namespace Spark.Tests.Precompiled
{
    [SparkView(
        TargetNamespace = "Spark.Tests.Precompiled", 
        Templates = new[] { "Foo\\Bar.spark", "Shared\\Quux.spark" })]
    public class View1 : StubSparkView
    {
        public View1(SparkViewBase<object> decorated)
            : base(decorated)
        {
        }

        public override void Render()
        {
            Output.Write("<p>Hello world</p>");
        }

        public override Guid GeneratedViewId
        {
            get { return new Guid("11111111123412341234123456123456"); }
        }

        public override bool TryGetViewData(string name, out object value)
        {
            throw new NotImplementedException();
        }
    }
}
