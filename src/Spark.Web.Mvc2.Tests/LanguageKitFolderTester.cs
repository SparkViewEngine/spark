using System.IO;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Spark.FileSystem;

namespace Spark.Web.Mvc.Tests
{
    [TestFixture]
    public class LanguageKitFolderTester
    {
        private LanguageKit.Folder _adapter;
        private InMemoryViewFolder _folder;

        [SetUp]
        public void Init()
        {
            _folder = new InMemoryViewFolder
                          {
                              {"Home\\Index.spark", "alpha"}
                          };
            _adapter = new LanguageKit.Folder(_folder);
        }

        [Test]
        public void ViewContentPassesThroughNormally()
        {
            Init();
            Assert.That(_adapter.HasView("Home\\Index.spark"), Is.True);
            Assert.That(_adapter.HasView("Home\\Index2.spark"), Is.False);

            var contents = ReadContents(_adapter, "Home\\Index.spark");
            Assert.That(contents, Is.EqualTo("alpha"));

            var items = _adapter.ListViews("Home");
            Assert.That(items.Count, Is.EqualTo(1));
            Assert.That(items[0], Is.EqualTo("Home\\Index.spark"));
        }

        private static string ReadContents(IViewFolder adapter, string path)
        {
            Assert.That(adapter.HasView(path), Is.True);
            var source = adapter.GetViewSource(path);
            var stream = source.OpenViewStream();
            var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }

        [Test]
        public void ParseLanguagePathTakesOffFirstTwoBits()
        {
            LanguageKit.LanguagePath path;

            path = _adapter.ParseLanguagePath("language\\en\\foo\\bar.spark");
            Assert.That(path, Is.Not.Null);
            Assert.That(path.Language, Is.EqualTo("en"));
            Assert.That(path.Path, Is.EqualTo("foo\\bar.spark"));

            path = _adapter.ParseLanguagePath("Language\\en-us\\foo\\bar.spark");
            Assert.That(path, Is.Not.Null);
            Assert.That(path.Language, Is.EqualTo("en-us"));
            Assert.That(path.Path, Is.EqualTo("foo\\bar.spark"));

            path = _adapter.ParseLanguagePath("notLanguage\\en-us\\foo\\bar.spark");
            Assert.That(path, Is.Null);

            path = _adapter.ParseLanguagePath("Languages\\en-us\\foo\\bar.spark");
            Assert.That(path, Is.Null);
        }

        [Test]
        public void ShortOrUnusualPathsWorkAsExpected()
        {
            LanguageKit.LanguagePath path;

            path = _adapter.ParseLanguagePath("language\\en\\");
            Assert.That(path, Is.Not.Null);

            path = _adapter.ParseLanguagePath("language\\en");
            Assert.That(path, Is.Null);

            path = _adapter.ParseLanguagePath("language\\");
            Assert.That(path, Is.Null);

            path = _adapter.ParseLanguagePath("languages");
            Assert.That(path, Is.Null);

            path = _adapter.ParseLanguagePath("language");
            Assert.That(path, Is.Null);
            
            path = _adapter.ParseLanguagePath("lang");
            Assert.That(path, Is.Null);
        }

        [Test]
        public void HasViewForLanguageFolderPathsWillHitNativeFiles()
        {
            Assert.That(_adapter.HasView("language\\en\\Home\\Index.spark"), Is.True);
            Assert.That(_adapter.HasView("language\\en-us\\Home\\Index.spark"), Is.True);
            Assert.That(_adapter.HasView("Language\\en\\Home\\Index.spark"), Is.True);
            Assert.That(_adapter.HasView("Language\\en-us\\Home\\Index.spark"), Is.True);
            Assert.That(_adapter.HasView("language\\en\\Home\\NoSuchFile.spark"), Is.False);
            Assert.That(_adapter.HasView("language\\en-us\\Home\\NoSuchFile.spark"), Is.False);
            Assert.That(_adapter.HasView("Language\\en\\Home\\NoSuchFile.spark"), Is.False);
            Assert.That(_adapter.HasView("Language\\en-us\\Home\\NoSuchFile.spark"), Is.False);
        }

        [Test]
        public void LanguageFolderPathsWillHitNativeFilesByName()
        {
            Init();

            Assert.That(ReadContents(_adapter, "language\\en\\Home\\Index.spark"), Is.EqualTo("alpha"));
            Assert.That(ReadContents(_adapter, "language\\en-us\\Home\\Index.spark"), Is.EqualTo("alpha"));
            Assert.That(ReadContents(_adapter, "Language\\en\\Home\\Index.spark"), Is.EqualTo("alpha"));
            Assert.That(ReadContents(_adapter, "Language\\en-us\\Home\\Index.spark"), Is.EqualTo("alpha"));
        }

        [Test]
        public void FilesWithExtendedNamesAreHitFirst()
        {
            Init();

            _folder.Add("Home\\Index.fr.spark", "beta");
            _folder.Add("Home\\Index.en.spark", "gamma");
            _folder.Add("Home\\Index.en-us.spark", "delta");

            Assert.That(ReadContents(_adapter, "language\\fr\\Home\\Index.spark"), Is.EqualTo("beta"));
            Assert.That(ReadContents(_adapter, "language\\es\\Home\\Index.spark"), Is.EqualTo("alpha"));
            Assert.That(ReadContents(_adapter, "language\\en\\Home\\Index.spark"), Is.EqualTo("gamma"));
            Assert.That(ReadContents(_adapter, "language\\en-us\\Home\\Index.spark"), Is.EqualTo("delta"));
            Assert.That(ReadContents(_adapter, "language\\en-uk\\Home\\Index.spark"), Is.EqualTo("gamma"));
        }

        [Test]
        public void ListViewsPassesThrough()
        {
            Init();
            AddShared();

            var items = _adapter.ListViews("Home\\Shared");
            Assert.That(items.Count, Is.EqualTo(5));
            Assert.That(items, Has.Member("Home\\Shared\\_foo.spark"));
            Assert.That(items, Has.Member("Home\\Shared\\_foo.en.spark"));
            Assert.That(items, Has.Member("Home\\Shared\\_foo.en-uk.spark"));
            Assert.That(items, Has.Member("Home\\Shared\\_bar.spark"));
        }

        private void AddShared()
        {
            _folder.Add("Home\\Shared\\_foo.spark", "1");
            _folder.Add("Home\\Shared\\_foo.en.spark", "2");
            _folder.Add("Home\\Shared\\_foo.en-uk.spark", "3");
            _folder.Add("Home\\Shared\\_bar.spark", "4");
            _folder.Add("Home\\Shared\\_quux.es.spark", "4");
        }

        [Test]
        public void MatchingLanguagesAreListed()
        {
            Init();
            AddShared();

            var items = _adapter.ListViews("language\\en\\Home\\Shared");
            Assert.That(items, Has.Member("language\\en\\Home\\Shared\\_foo.spark"));
            Assert.That(items, Has.Member("language\\en\\Home\\Shared\\_bar.spark"));
            Assert.That(items, Has.No.Member("language\\es\\Home\\Shared\\_quux.spark"));
            Assert.That(items, Has.No.Member("Language\\es\\Home\\Shared\\_quux.spark"));

            items = _adapter.ListViews("language\\en-uk\\Home\\Shared");
            Assert.That(items, Has.Member("language\\en-uk\\Home\\Shared\\_foo.spark"));
            Assert.That(items, Has.Member("language\\en-uk\\Home\\Shared\\_bar.spark"));
            Assert.That(items, Has.No.Member("language\\es\\Home\\Shared\\_quux.spark"));
            Assert.That(items, Has.No.Member("Language\\es\\Home\\Shared\\_quux.spark"));

            items = _adapter.ListViews("Language\\es\\Home\\Shared");
            Assert.That(items, Has.Member("Language\\es\\Home\\Shared\\_foo.spark"));
            Assert.That(items, Has.Member("Language\\es\\Home\\Shared\\_bar.spark"));
            Assert.That(items, Has.Member("Language\\es\\Home\\Shared\\_quux.spark"));
        }
    }
}