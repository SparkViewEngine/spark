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

using NUnit.Framework;
using System.IO;

namespace Spark.FileSystem
{
    [TestFixture]
    public class SubViewFolderTester
    {
        [Test]
        public void SharingExtraFolders()
        {
            var normal = new FileSystemViewFolder("Spark.Tests.Views");
            var otherLocation = new FileSystemViewFolder(Path.Combine("Spark.Tests.Views", "Prefix"));

            var viewFolder = new CombinedViewFolder(normal, new SubViewFolder(otherLocation, "Shared"));

            var normalSharedCount = normal.ListViews("Shared").Count;
            var otherLocationCount = otherLocation.ListViews("").Count;
            var totalSharedCount = viewFolder.ListViews("Shared").Count;

            Assert.That(totalSharedCount, Is.EqualTo(normalSharedCount + otherLocationCount));
        }

        [Test]
        [Ignore("Is this test relevant after mono changes? : ahjohannessen")]
        public void ForwardAndBackSlashAreInterchangeable()
        {
            var viewsFolder = new InMemoryViewFolder
            {
                {@"Home\Index.spark", "1"},
                {@"Shared\_global.spark", "2"},
                {@"Collision\Home\Baaz.spark", "6"}
            };
            var extraFolder = new InMemoryViewFolder
            {
                {@"Home\Foo.spark", "3"},
                {@"Home\Bar.spark", "4"},
                {@"Home\Quux.spark", "5"},
            };

            Assert.That(viewsFolder.ListViews(@"Collision/Home").Count, Is.EqualTo(1));
            Assert.That(viewsFolder.ListViews(@"Collision\Home").Count, Is.EqualTo(1));
            Assert.That(extraFolder.ListViews(@"Home").Count, Is.EqualTo(3));

            var combinedFolder = viewsFolder
                .Append(new SubViewFolder(extraFolder, @"Extra/One"))
                .Append(new SubViewFolder(extraFolder, @"Extra\Two"))
                .Append(new SubViewFolder(extraFolder, @"Collision"));


            Assert.That(combinedFolder.ListViews("Home").Count, Is.EqualTo(1));
            Assert.That(combinedFolder.ListViews(@"Extra/One/Home").Count, Is.EqualTo(3));
            Assert.That(combinedFolder.ListViews(@"Extra\One/Home").Count, Is.EqualTo(3));
            Assert.That(combinedFolder.ListViews(@"Extra/One\Home").Count, Is.EqualTo(3));
            Assert.That(combinedFolder.ListViews(@"Extra\One\Home").Count, Is.EqualTo(3));
            Assert.That(combinedFolder.ListViews(@"Extra/Two/Home").Count, Is.EqualTo(3));
            Assert.That(combinedFolder.ListViews(@"Extra\Two/Home").Count, Is.EqualTo(3));
            Assert.That(combinedFolder.ListViews(@"Extra/Two\Home").Count, Is.EqualTo(3));
            Assert.That(combinedFolder.ListViews(@"Extra\Two\Home").Count, Is.EqualTo(3));
            Assert.That(combinedFolder.ListViews(@"Collision/Home").Count, Is.EqualTo(4));
            Assert.That(combinedFolder.ListViews(@"Collision\Home").Count, Is.EqualTo(4));

            Assert.Multiple(() =>
            {
                Assert.That(combinedFolder.HasView(@"Extra/One/Home/Bar.spark"), Is.True);
                Assert.That(combinedFolder.HasView(@"Extra\One\Home\Bar.spark"), Is.True);
                Assert.That(combinedFolder.HasView(@"Extra/Two/Home/Bar.spark"), Is.True);
                Assert.That(combinedFolder.HasView(@"Extra\Two\Home\Bar.spark"), Is.True);
            });

        }
    }
}
