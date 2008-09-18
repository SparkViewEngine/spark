using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TextManager.Interop;

namespace SparkVsPackage
{
    static class ColorableItemList
    {
        // The order of these items matches the SparkTokenType collection
        private static readonly IList<ColorableItem> _items = new[]
        {
            new ColorableItem(),

            new ColorableItem {Name="Spark Attribute Name", 
                Foreground=COLORINDEX.CI_RED},

            new ColorableItem {Name="Spark Attribute Quotes", 
                Foreground=COLORINDEX.CI_USERTEXT_FG},
            
            new ColorableItem {Name="Spark Attribute Value", 
                Foreground=COLORINDEX.CI_BLUE},

            new ColorableItem {Name="Spark CDATA Section", 
                Foreground=COLORINDEX.CI_DARKGRAY},
            
            new ColorableItem {Name="Spark Comment", 
                Foreground=COLORINDEX.CI_DARKGREEN},

            new ColorableItem {Name="Spark Delimiter", 
                Foreground=COLORINDEX.CI_BLUE},

            new ColorableItem {Name="Spark Keyword", 
                Foreground=COLORINDEX.CI_BLUE},

            new ColorableItem {Name="Spark Code", 
                Foreground=COLORINDEX.CI_USERTEXT_FG},

            new ColorableItem {Name="Spark Element Name", 
                Foreground=COLORINDEX.CI_MAROON},

            new ColorableItem {Name="Spark Text", 
                Foreground=COLORINDEX.CI_USERTEXT_FG},

            new ColorableItem {Name="Spark Processing Instruction", 
                Foreground=COLORINDEX.CI_DARKGRAY},

            new ColorableItem {Name="Spark String", 
                Foreground=COLORINDEX.CI_MAROON},
       };


        public static IList<ColorableItem> Items
        {
            get { return _items; }
        }
    }
}
