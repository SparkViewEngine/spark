using System;
using System.Linq;
using NUnit.Framework;
using Spark.Parser.Markup;
using SparkLanguagePackageLib;
using IVsPackage = Microsoft.VisualStudio.Shell.Interop.IVsPackage;


namespace SparkLanguage.Tests
{
    [TestFixture]
    public class ColorableTester
    {
        private StubPackageSite _packageSite;

        [SetUp]
        public void Init()
        {
            _packageSite = new StubPackageSite();
        }

        static int EnumColorCount()
        {
            var length = Enum.GetValues(typeof(SparkTokenType)).Length;
            return length - 1;
        }

        [Test]
        public void ColorEnumMaxValueAndCountAreSame()
        {
            var count = Enum.GetValues(typeof (SparkTokenType)).Length;
            var max = Enum.GetValues(typeof (SparkTokenType)).Cast<SparkTokenType>().Max();
            Assert.AreEqual(count - 1, (int)max);
        }

        [Test]
        public void CorrectNumberOfColorsReturn()
        {
            var colorableItems = (IVsProvideColorableItems)new LanguageSupervisor();

            int iCount;
            colorableItems.GetItemCount(out iCount);

            Assert.AreEqual(EnumColorCount(), iCount);

            var items = new IVsColorableItem[iCount];
            for (var index = 0; index != iCount; ++index)
            {
                // color indexes are one based, 0 is reserved for default plain text
                colorableItems.GetColorableItem(index + 1, out items[index]);
            }
        }

        [Test]
        public void PackageReturnsContainedLanguageAndSparkColorsTogether()
        {
            var sparkPackage = (IVsPackage)new PackageClass();
            sparkPackage.SetSite(_packageSite);

            var colorableItems = _packageSite.QueryService<IVsProvideColorableItems, SparkLanguageService>();

            int iCount;
            colorableItems.GetItemCount(out iCount);

            Assert.AreEqual(13, iCount);

            var items = new IVsColorableItem[iCount + 1];
            for (var index = 1; index != iCount + 1; ++index)
            {
                // color indexes are one based, 0 is reserved for default plain text
                colorableItems.GetColorableItem(index, out items[index]);
            }

            string name;

            items[1].GetDisplayName(out name);
            Assert.AreEqual("Stub 1", name);
            
            items[2].GetDisplayName(out name);
            Assert.AreEqual("Stub 2", name);
            
            items[3].GetDisplayName(out name);
            Assert.AreEqual("Stub 3", name);

            Assert.AreEqual(1, (int)SparkTokenType.HtmlTagDelimiter);
            items[4].GetDisplayName(out name);
            Assert.AreEqual("Spark HTML Tag Delimiter", name);

            Assert.AreEqual(10, (int)SparkTokenType.SparkDelimiter);
            items[13].GetDisplayName(out name);
            Assert.AreEqual("Spark Code Delimiter", name);
        }
    }
}
