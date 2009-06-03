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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Spark.FileSystem;

namespace Spark.Tests.FileSystem
{
    [TestFixture]
    public class SubViewFolderTester
    {
        [Test]
        public void SharingExtraFolders()
        {
            var normal = new FileSystemViewFolder("Spark.Tests.Views");
            var otherLocation = new FileSystemViewFolder("Spark.Tests.Views\\Prefix".AsPath());

            var viewFolder = new CombinedViewFolder(normal, new SubViewFolder(otherLocation, "Shared"));

            var normalSharedCount = normal.ListViews("Shared").Count;
            var otherLocationCount = otherLocation.ListViews("").Count;
            var totalSharedCount = viewFolder.ListViews("Shared").Count;

            Assert.AreEqual(normalSharedCount + otherLocationCount, totalSharedCount);
        }

        [Test]
        public void ForwardAndBackSlashAreInterchangeable()
        {
            // note- this test is a special case as far as pathing goes. 
            // on .net it's expected to show that \ and / are interchangeable. 
            // on mono it's expected to test entirely / cases because \ can be in a segment name.

            var viewsFolder = new InMemoryViewFolder
                                  {
                                      {@"Home\Index.spark".AsPath(), "1"},
                                      {@"Shared\_global.spark".AsPath(), "2"},
                                      {@"Collision\Home\Baaz.spark".AsPath(), "6"}
                                  };
            var extraFolder = new InMemoryViewFolder
                                  {
                                      {@"Home\Foo.spark".AsPath(), "3"},
                                      {@"Home\Bar.spark".AsPath(), "4"},
                                      {@"Home\Quux.spark".AsPath(), "5"},
                                  };

            Assert.AreEqual(1, viewsFolder.ListViews(@"Collision/Home".AsPath()).Count);
            Assert.AreEqual(1, viewsFolder.ListViews(@"Collision\Home".AsPath()).Count);
            Assert.AreEqual(3, extraFolder.ListViews(@"Home".AsPath()).Count);

            var combinedFolder = viewsFolder
                .Append(new SubViewFolder(extraFolder, @"Extra/One".AsPath()))
                .Append(new SubViewFolder(extraFolder, @"Extra\Two".AsPath()))
                .Append(new SubViewFolder(extraFolder, @"Collision".AsPath()));


            Assert.AreEqual(1, combinedFolder.ListViews("Home".AsPath()).Count);
            Assert.AreEqual(3, combinedFolder.ListViews(@"Extra/One/Home".AsPath()).Count);
            Assert.AreEqual(3, combinedFolder.ListViews(@"Extra\One/Home".AsPath()).Count);
            Assert.AreEqual(3, combinedFolder.ListViews(@"Extra/One\Home".AsPath()).Count);
            Assert.AreEqual(3, combinedFolder.ListViews(@"Extra\One\Home".AsPath()).Count);
            Assert.AreEqual(3, combinedFolder.ListViews(@"Extra/Two/Home".AsPath()).Count);
            Assert.AreEqual(3, combinedFolder.ListViews(@"Extra\Two/Home".AsPath()).Count);
            Assert.AreEqual(3, combinedFolder.ListViews(@"Extra/Two\Home".AsPath()).Count);
            Assert.AreEqual(3, combinedFolder.ListViews(@"Extra\Two\Home".AsPath()).Count);
            Assert.AreEqual(4, combinedFolder.ListViews(@"Collision/Home".AsPath()).Count);
            Assert.AreEqual(4, combinedFolder.ListViews(@"Collision\Home".AsPath()).Count);

            Assert.IsTrue(combinedFolder.HasView(@"Extra/One/Home/Bar.spark".AsPath()));
            Assert.IsTrue(combinedFolder.HasView(@"Extra\One\Home\Bar.spark".AsPath()));
            Assert.IsTrue(combinedFolder.HasView(@"Extra/Two/Home/Bar.spark".AsPath()));
            Assert.IsTrue(combinedFolder.HasView(@"Extra\Two\Home\Bar.spark".AsPath()));

        }
    }
}
