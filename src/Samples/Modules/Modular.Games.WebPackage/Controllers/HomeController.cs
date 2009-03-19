using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Modular.Common.Services;

namespace Modular.Games.WebPackage.Controllers
{
    public class HomeController : Controller
    {
        private readonly IGameRegistry _gameRegistry;
        private readonly IHighScoreRepository _highScoreRepository;

        public HomeController(IGameRegistry gameRegistry, IHighScoreRepository highScoreRepository)
        {
            _gameRegistry = gameRegistry;
            _highScoreRepository = highScoreRepository;
        }

        public ActionResult Index()
        {
            return View(_gameRegistry.ListGames());
        }

        public ActionResult Scores()
        {
            return View(_highScoreRepository.ListScores());
        }
    }
}
