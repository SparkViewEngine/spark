using System;
using SparkLanguagePackageLib;

namespace SparkLanguage.Tests
{
    internal class StubContainedLanguageService : IVsProvideColorableItems
    {
        public void GetItemCount(out int piCount)
        {
            piCount = 3;
        }

        public void GetColorableItem(int iIndex, out IVsColorableItem ppItem)
        {
            if (iIndex < 1 || iIndex > 3)
                throw new IndexOutOfRangeException();
            ppItem = new StubContainedColorableItem(iIndex);
        }
    }
}