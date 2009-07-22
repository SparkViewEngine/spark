using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Spark;

namespace CachingViewHunks.Models
{
    public class Tile
    {
        public int TileId { get; set; }
        public string Text { get; set; }
        public CacheSignal Signal { get; set; }
    }
}
