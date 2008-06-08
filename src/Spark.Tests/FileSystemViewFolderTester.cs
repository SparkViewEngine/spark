using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using NUnit.Framework;
using Spark.FileSystem;

namespace Spark.Tests
{
	[TestFixture]
	public class FileSystemViewFolderTester
	{
		private FileSystemViewFolder _viewFolder;

		[SetUp]
		public void Init()
		{
			_viewFolder = new FileSystemViewFolder("Views");
		}

		[Test]
		public void HasViewBoolean()
		{
			var fileExists = _viewFolder.HasView("Home\\foreach.xml");
			var fileNotFound = _viewFolder.HasView("Home\\fakefile.xml");
			Assert.IsTrue(fileExists);
			Assert.IsFalse(fileNotFound);
		}

		[Test]
		public void ListingViewsInFolder()
		{
			var shared = _viewFolder.ListViews("Shared");
			Assert.That(shared.Contains("_comment.xml"));
			Assert.That(shared.Contains("layout.xml"));
			Assert.That(shared.Contains("partial.xml"));
		}

		[Test, ExpectedException(typeof(FileNotFoundException))]
		public void GetSourceNotFound()
		{
			_viewFolder.GetViewSource("Home\\NoSuchFile.xml");
		}


		[Test]
		public void ReadingFileContents()
		{
			var viewSource = _viewFolder.GetViewSource("Home\\foreach.xml");
			var reader = new StreamReader(viewSource.OpenViewStream());
			var contents = reader.ReadToEnd();
			Assert.That(contents.Contains("<for each="));
		}

		[Test]
		public void LastModifiedChanges()
		{
			var viewSource = _viewFolder.GetViewSource("Home\\foreach.xml");
			var lastModified1 = viewSource.LastModified;
	
			Thread.Sleep(TimeSpan.FromMilliseconds(75));
			var lastModified2 = viewSource.LastModified;

			Assert.AreEqual(lastModified1, lastModified2);

			File.SetLastWriteTimeUtc("Views\\Home\\foreach.xml", DateTime.UtcNow);
			var lastModified3 = viewSource.LastModified;

			Assert.AreNotEqual(lastModified1, lastModified3);
		}
	}
}
