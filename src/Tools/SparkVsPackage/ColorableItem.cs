using Microsoft.VisualStudio;
using Microsoft.VisualStudio.TextManager.Interop;

namespace SparkVsPackage
{
    class ColorableItem : IVsColorableItem
    {
        public ColorableItem()
        {
            Foreground = COLORINDEX.CI_USERTEXT_FG;
            Background = COLORINDEX.CI_USERTEXT_BK;
        }
        public string Name { get; set; }
        public COLORINDEX Foreground { get; set; }
        public COLORINDEX Background { get; set; }

        public int GetDefaultColors(COLORINDEX[] piForeground, COLORINDEX[] piBackground)
        {
            piForeground[0] = Foreground;
            piBackground[0] = Background;
            return VSConstants.S_OK;
        }


        public int GetDefaultFontFlags(out uint pdwFontFlags)
        {
            pdwFontFlags = 0;
            return VSConstants.S_OK;
        }

        public int GetDisplayName(out string pbstrName)
        {
            pbstrName = Name;
            return VSConstants.S_OK;
        }
    }
}