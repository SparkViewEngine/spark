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
                              {string.Format("Home{0}Index.spark", Path.DirectorySeparatorChar), "alpha"}
                          };
            _adapter = new LanguageKit.Folder(_folder);
        }

        [Test]
        public void ViewContentPassesThroughNormally()
        {
            Init();
            Assert.That(_adapter.HasView(string.Format("Home{0}Index.spark", Path.DirectorySeparatorChar)), Is.True);
            Assert.That(_adapter.HasView(string.Format("Home{0}Index2.spark", Path.DirectorySeparatorChar)), Is.False);

            var contents = ReadContents(_adapter, string.Format("Home{0}Index.spark", Path.DirectorySeparatorChar));
            Assert.That(contents, Is.EqualTo("alpha"));

            var items = _adapter.ListViews("Home");
            Assert.That(items.Count, Is.EqualTo(1));
            Assert.That(items[0], Is.EqualTo(string.Format("Home{0}Index.spark", Path.DirectorySeparatorChar)));
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

            path = _adapter.ParseLanguagePath(string.Format("language{0}en{0}foo{0}bar.spark", Path.DirectorySeparatorChar));
            Assert.That(path, Is.Not.Null);
            Assert.That(path.Language, Is.EqualTo("en"));
            Assert.That(path.Path, Is.EqualTo(string.Format("foo{0}bar.spark", Path.DirectorySeparatorChar)));

            path = _adapter.ParseLanguagePath(string.Format("Language{0}en-us{0}foo{0}bar.spark", Path.DirectorySeparatorChar));
            Assert.That(path, Is.Not.Null);
            Assert.That(path.Language, Is.EqualTo("en-us"));
            Assert.That(path.Path, Is.EqualTo(string.Format("foo{0}bar.spark", Path.DirectorySeparatorChar)));

            path = _adapter.ParseLanguagePath(string.Format("notLanguage{0}en-us{0}foo{0}bar.spark", Path.DirectorySeparatorChar));
            Assert.That(path, Is.Null);

            path = _adapter.ParseLanguagePath(string.Format("Languages{0}en-us{0}foo{0}bar.spark", Path.DirectorySeparatorChar));
            Assert.That(path, Is.Null);
        }

        [Test]
        public void ShortOrUnusualPathsWorkAsExpected()
        {
            LanguageKit.LanguagePath path;

            path = _adapter.ParseLanguagePath(string.Format("language{0}en{0}", Path.DirectorySeparatorChar));
            Assert.That(path, Is.Not.Null);

            path = _adapter.ParseLanguagePath(string.Format("language{0}en", Path.DirectorySeparatorChar));
            Assert.That(path, Is.Null);

            path = _adapter.ParseLanguagePath(string.Format("language{0}", Path.DirectorySeparatorChar));
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
            Assert.That(_adapter.HasView(string.Format("language{0}en{0}Home{0}Index.spark", Path.DirectorySeparatorChar)), Is.True);
            Assert.That(_adapter.HasView(string.Format("language{0}en-us{0}Home{0}Index.spark", Path.DirectorySeparatorChar)), Is.True);
            Assert.That(_adapter.HasView(string.Format("Language{0}en{0}Home{0}Index.spark", Path.DirectorySeparatorChar)), Is.True);
            Assert.That(_adapter.HasView(string.Format("Language{0}en-us{0}Home{0}Index.spark", Path.DirectorySeparatorChar)), Is.True);
            Assert.That(_adapter.HasView(string.Format("language{0}en{0}Home{0}NoSuchFile.spark", Path.DirectorySeparatorChar)), Is.False);
            Assert.That(_adapter.HasView(string.Format("language{0}en-us{0}Home{0}NoSuchFile.spark", Path.DirectorySeparatorChar)), Is.False);
            Assert.That(_adapter.HasView(string.Format("Language{0}en{0}Home{0}NoSuchFile.spark", Path.DirectorySeparatorChar)), Is.False);
            Assert.That(_adapter.HasView(string.Format("Language{0}en-us{0}Home{0}NoSuchFile.spark", Path.DirectorySeparatorChar)), Is.False);
        }

        [Test]
        public void LanguageFolderPathsWillHitNativeFilesByName()
        {
            Init();

            Assert.That(ReadContents(_adapter, string.Format("language{0}en{0}Home{0}Index.spark", Path.DirectorySeparatorChar)), Is.EqualTo("alpha"));
            Assert.That(ReadContents(_adapter, string.Format("language{0}en-us{0}Home{0}Index.spark", Path.DirectorySeparatorChar)), Is.EqualTo("alpha"));
            Assert.That(ReadContents(_adapter, string.Format("Language{0}en{0}Home{0}Index.spark", Path.DirectorySeparatorChar)), Is.EqualTo("alpha"));
            Assert.That(ReadContents(_adapter, string.Format("Language{0}en-us{0}Home{0}Index.spark", Path.DirectorySeparatorChar)), Is.EqualTo("alpha"));
        }

        [Test]
        public void FilesWithExtendedNamesAreHitFirst()
        {
            Init();

            _folder.Add(string.Format("Home{0}Index.fr.spark", Path.DirectorySeparatorChar), "beta");
            _folder.Add(string.Format("Home{0}Index.en.spark", Path.DirectorySeparatorChar), "gamma");
            _folder.Add(string.Format("Home{0}Index.en-us.spark", Path.DirectorySeparatorChar), "delta");

            Assert.That(ReadContents(_adapter, string.Format("language{0}fr{0}Home{0}Index.spark", Path.DirectorySeparatorChar)), Is.EqualTo("beta"));
            Assert.That(ReadContents(_adapter, string.Format("language{0}es{0}Home{0}Index.spark", Path.DirectorySeparatorChar)), Is.EqualTo("alpha"));
            Assert.That(ReadContents(_adapter, string.Format("language{0}en{0}Home{0}Index.spark", Path.DirectorySeparatorChar)), Is.EqualTo("gamma"));
            Assert.That(ReadContents(_adapter, string.Format("language{0}en-us{0}Home{0}Index.spark", Path.DirectorySeparatorChar)), Is.EqualTo("delta"));
            Assert.That(ReadContents(_adapter, string.Format("language{0}en-uk{0}Home{0}Index.spark", Path.DirectorySeparatorChar)), Is.EqualTo("gamma"));
        }

        [Test]
        public void ListViewsPassesThrough()
        {
            Init();
            AddShared();

            var items = _adapter.ListViews(string.Format("Home{0}Shared", Path.DirectorySeparatorChar));
            Assert.That(items.Count, Is.EqualTo(5));
            Assert.That(items, Has.Member(string.Format("Home{0}Shared{0}_foo.spark", Path.DirectorySeparatorChar)));
            Assert.That(items, Has.Member(string.Format("Home{0}Shared{0}_foo.en.spark", Path.DirectorySeparatorChar)));
            Assert.That(items, Has.Member(string.Format("Home{0}Shared{0}_foo.en-uk.spark", Path.DirectorySeparatorChar)));
            Assert.That(items, Has.Member(string.Format("Home{0}Shared{0}_bar.spark", Path.DirectorySeparatorChar)));
        }

        private void AddShared()
        {
            _folder.Add(string.Format("Home{0}Shared{0}_foo.spark", Path.DirectorySeparatorChar), "1");
            _folder.Add(string.Format("Home{0}Shared{0}_foo.en.spark", Path.DirectorySeparatorChar), "2");
            _folder.Add(string.Format("Home{0}Shared{0}_foo.en-uk.spark", Path.DirectorySeparatorChar), "3");
            _folder.Add(string.Format("Home{0}Shared{0}_bar.spark", Path.DirectorySeparatorChar), "4");
            _folder.Add(string.Format("Home{0}Shared{0}_quux.es.spark", Path.DirectorySeparatorChar), "4");
        }

        [Test]
        public void MatchingLanguagesAreListed()
        {
            Init();
            AddShared();

            var items = _adapter.ListViews(string.Format("language{0}en{0}Home{0}Shared", Path.DirectorySeparatorChar));
            Assert.That(items, Has.Member(string.Format("language{0}en{0}Home{0}Shared{0}_foo.spark", Path.DirectorySeparatorChar)));
            Assert.That(items, Has.Member(string.Format("language{0}en{0}Home{0}Shared{0}_bar.spark", Path.DirectorySeparatorChar)));
            Assert.That(items, Has.No.Member(string.Format("language{0}es{0}Home{0}Shared{0}_quux.spark", Path.DirectorySeparatorChar)));
            Assert.That(items, Has.No.Member(string.Format("Language{0}es{0}Home{0}Shared{0}_quux.spark", Path.DirectorySeparatorChar)));

            items = _adapter.ListViews(string.Format("language{0}en-uk{0}Home{0}Shared", Path.DirectorySeparatorChar));
            Assert.That(items, Has.Member(string.Format("language{0}en-uk{0}Home{0}Shared{0}_foo.spark", Path.DirectorySeparatorChar)));
            Assert.That(items, Has.Member(string.Format("language{0}en-uk{0}Home{0}Shared{0}_bar.spark", Path.DirectorySeparatorChar)));
            Assert.That(items, Has.No.Member(string.Format("language{0}es{0}Home{0}Shared{0}_quux.spark", Path.DirectorySeparatorChar)));
            Assert.That(items, Has.No.Member(string.Format("Language{0}es{0}Home{0}Shared{0}_quux.spark", Path.DirectorySeparatorChar)));

            items = _adapter.ListViews(string.Format("Language{0}es{0}Home{0}Shared", Path.DirectorySeparatorChar));
            Assert.That(items, Has.Member(string.Format("Language{0}es{0}Home{0}Shared{0}_foo.spark", Path.DirectorySeparatorChar)));
            Assert.That(items, Has.Member(string.Format("Language{0}es{0}Home{0}Shared{0}_bar.spark", Path.DirectorySeparatorChar)));
            Assert.That(items, Has.Member(string.Format("Language{0}es{0}Home{0}Shared{0}_quux.spark", Path.DirectorySeparatorChar)));
        }
    }
}