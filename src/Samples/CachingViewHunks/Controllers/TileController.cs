using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CachingViewHunks.Models;

namespace CachingViewHunks.Controllers
{
    public class TileController : Controller
    {
        public ActionResult Index()
        {
            var repos = new TileRepository();
            ViewData["tiles"] = repos.GetTiles();
            return View();
        }

        public ActionResult Scramble(int id)
        {
            var repos = new TileRepository();
            var tile = repos.GetTile(id);

            var random = new Random();
            var characters = tile.Text
                .ToCharArray()
                .Select(ch => new { ch, order = random.NextDouble() })
                .OrderBy(x => x.order)
                .Select(x => x.ch);

            repos.ChangeTile(id, new string(characters.ToArray()));

            return RedirectToAction("index");
        }

        public ActionResult SignalAll()
        {
            var repos = new TileRepository();
            foreach (var tile in repos.GetTiles())
                tile.Signal.FireChanged();

            return RedirectToAction("index");
        }

        public ActionResult ResetAll()
        {
            var repos = new TileRepository();
            var tileData = "the quick brown fox jumped over the sleepy dog"
                .Split(' ')
                .Select((text, index) => new { text, index });

            foreach (var data in tileData)
            {
                if (repos.GetTile(data.index).Text != data.text)
                    repos.ChangeTile(data.index, data.text);
            }

            return RedirectToAction("index");
        }
    }
}
