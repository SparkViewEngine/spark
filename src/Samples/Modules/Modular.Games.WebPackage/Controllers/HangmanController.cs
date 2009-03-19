using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Web.Routing;
using Modular.Common.Services;
using Modular.Games.WebPackage.Models;

namespace Modular.Games.WebPackage.Controllers
{
    public class HangmanController : Controller
    {
        private readonly IWordProvider _wordProvider;
        private readonly IHighScoreRepository _highScoreRepository;

        public HangmanController(IWordProvider wordProvider, IHighScoreRepository highScoreRepository)
        {
            _wordProvider = wordProvider;
            _highScoreRepository = highScoreRepository;
        }

        public HangmanGame Game { get; set; }

        [HangmanGameRequired]
        public ActionResult Play()
        {
            return View("ShowGame", Game);
        }

        public ActionResult Reset()
        {
            Game = new HangmanGame(_wordProvider.GetRandomWord());
            Session["HangmanGame"] = Game;
            return View("ShowGame", Game);
        }

        [HangmanGameRequired]
        public ActionResult Guess(string letter)
        {
            var finished = Game.Victory;

            Game.Guess(letter);

            if (Game.Victory && !finished)
            {
                _highScoreRepository.AddHighScore(
                    DateTime.UtcNow, 
                    "Letter Guess - " + Game.Answer, 
                    Game.Moves, 
                    Game,
                    new { controller = "hangman", area = "games", id = Game.Id });
            }

            return View("ShowGame", Game);
        }

        public ActionResult Review(Guid id)
        {
            var game = _highScoreRepository.ListScores()
                .Select(e => e.GameState)
                .OfType<HangmanGame>()
                .Single(g => g.Id == id);

            return View("ShowGame", game);
        }
    }

    public class HangmanGameRequiredAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var game = (HangmanGame)filterContext.HttpContext.Session["HangmanGame"];
            if (game == null)
            {
                filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary(
                    new { action = "reset", controller = "hangman", area = "games" }));
                return;
            }
            ((HangmanController)filterContext.Controller).Game = game;
        }
    }
}
