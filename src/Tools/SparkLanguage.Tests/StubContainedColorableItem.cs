using System;
using SparkLanguagePackageLib;

namespace SparkLanguage.Tests
{
    public class StubContainedColorableItem : MarshalByRefObject, IVsColorableItem
    {
        private readonly string _name;

        public StubContainedColorableItem(int index)
        {
            _name = "Stub " + index;
        }

        public void GetDefaultColors(out _COLORINDEX piForeground, out _COLORINDEX piBackground)
        {
            throw new System.NotImplementedException();
        }

        public void GetDefaultFontFlags(out uint pdwFontFlags)
        {
            throw new System.NotImplementedException();
        }

        public void GetDisplayName(out string pbstrName)
        {
            pbstrName = _name;
        }
    }
}