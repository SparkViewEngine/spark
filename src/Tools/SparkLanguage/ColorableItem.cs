using System;
using SparkLanguagePackageLib;

namespace SparkLanguage
{
    public class ColorableItem : MarshalByRefObject, IVsColorableItem
    {
        public ColorableItem(string displayName, _COLORINDEX foreground)
            : this(displayName, foreground, _COLORINDEX.CI_USERTEXT_BK)
        {
        }
        public ColorableItem(string displayName, _COLORINDEX foreground, _COLORINDEX background)
        {
            DisplayName = displayName;
            Foreground = foreground;
            Background = background;
            FontFlags = 0;
        }

        public string DisplayName { get; set; }
        public _COLORINDEX Foreground { get; set; }
        public _COLORINDEX Background { get; set; }
        public uint FontFlags { get; set; }

        public void GetDisplayName(out string pbstrName)
        {
            pbstrName = DisplayName;
        }

        public void GetDefaultColors(out _COLORINDEX piForeground, out _COLORINDEX piBackground)
        {
            piForeground = Foreground;
            piBackground = Background;
        }

        public void GetDefaultFontFlags(out uint pdwFontFlags)
        {
            pdwFontFlags = FontFlags;
        }
    }
}