using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Spark;

namespace CachingViewHunks.Models
{
    public class TileRepository
    {
        static TileRepository()
        {
            _tileData = "the quick brown fox jumped over the sleepy dog".Split(' ');
            _tileSignals = _tileData.Select(x => new CacheSignal()).ToArray();
        }

        private static readonly string[] _tileData;
        private static readonly CacheSignal[] _tileSignals;

        public IEnumerable<Tile> GetTiles()
        {
            return _tileData.Select(
                (data, index) => new Tile
                                 {
                                     TileId = index,
                                     Text = data,
                                     Signal = _tileSignals[index]
                                 });
        }

        public Tile GetTile(int tileId)
        {
            return GetTiles().Single(x => x.TileId == tileId);
        }

        public void ChangeTile(int tileId, string text)
        {
            _tileData[tileId] = text;
            _tileSignals[tileId].FireChanged();
        }
    }
}
