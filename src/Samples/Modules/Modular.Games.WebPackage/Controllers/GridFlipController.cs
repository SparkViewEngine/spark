using System;
using System.Linq;
using System.Web.Mvc;
using Modular.Common.Services;
using Modular.Games.WebPackage.Models;

namespace Modular.Games.WebPackage.Controllers
{
    public class GridFlipController : Controller
    {
        private readonly IHighScoreRepository _highScoreRepository;

        public GridFlipController(IHighScoreRepository highScoreRepository)
        {
            _highScoreRepository = highScoreRepository;
        }

        public ActionResult Play()
        {
            return Index();
        }

        public ActionResult Index()
        {
            var game = Session["GridGame"] as GridGame;
            if (game == null)
                return RedirectToAction("Reset", "GridFlip");

            return View("ShowGrid", game);
        }

        public ActionResult Reset()
        {
            var game = new GridGame();
            game.Randomize();
            Session["GridGame"] = game;

            return View("ShowGrid", game);
        }

        public ActionResult Toggle(int cell)
        {
            var game = Session["GridGame"] as GridGame;
            if (game == null)
                return RedirectToAction("Reset", "GridFlip");

            var finished = game.Victory;


            if (game.Victory == false)
                game.Moves += 1;

            foreach (var flip in new[]
                                     {
                                         new[] {0, 1, 3},
                                         new[] {0, 1, 2},
                                         new[] {1, 2, 5},
                                         new[] {0, 3, 6},
                                         new[] {0, 2, 4, 6, 8},
                                         new[] {2, 5, 8},
                                         new[] {3, 6, 7},
                                         new[] {6, 7, 8},
                                         new[] {5, 7, 8},
                                     }[cell])
            {

                game.Cells[flip] = !game.Cells[flip];
            }

            var target = new[]
                                {
                                    true, true, true,
                                    true, false, true,
                                    true, true, true
                                };

            if (game.Cells.Where((state, index) => target[index] != state).Count() == 0)
                game.Victory = true;

            if (game.Victory && !finished)
            {
                _highScoreRepository.AddHighScore(
                    DateTime.UtcNow,
                    "Grid Flip",
                    game.Moves,
                    game,
                    new { controller = "gridflip", area = "games", id = game.Id });
            }

            return View("ShowGrid", game);
        }

        public ActionResult Review(Guid id)
        {
            var game = _highScoreRepository.ListScores()
                .Select(e => e.GameState)
                .OfType<GridGame>()
                .Single(g => g.Id == id);

            return View("ShowGrid", game);
        }
    }
}
