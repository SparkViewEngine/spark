// Copyright 2008-2009 Louis DeJardin - http://whereslou.com
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
using Castle.MonoRail.Framework;
using Castle.MonoRail.Views.Spark.Wrappers;
using NUnit.Framework;
using Rhino.Mocks;

namespace Castle.MonoRail.Views.Spark.Tests
{
    [TestFixture]
    public class ViewSourceLoaderWrapperTests
    {
        #region Setup/Teardown

        [SetUp]
        public void Init()
        {
            var container = MockRepository.GenerateStub<IViewSourceLoaderContainer>();
            container.ViewSourceLoader = MockRepository.GenerateStub<IViewSourceLoader>();
            container.ViewSourceLoader.Stub(x => x.GetViewSource(NonExistingView)).Return(null);
            _viewFolder = new ViewSourceLoaderWrapper(container);
        }

        #endregion

        private ViewSourceLoaderWrapper _viewFolder;
        private readonly string NonExistingView = string.Format("Home{0}IDoNotExist", Path.DirectorySeparatorChar);

        [Test, ExpectedException(typeof (FileNotFoundException))]
        public void GetSourceNotFound()
        {
            _viewFolder.GetViewSource(NonExistingView);
        }
    }
}