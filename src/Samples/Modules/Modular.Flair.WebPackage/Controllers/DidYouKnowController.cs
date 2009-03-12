using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Modular.Flair.WebPackage.Services;

namespace Modular.Flair.WebPackage.Controllers
{
    public class DidYouKnowController : Controller
    {
        private readonly ITriviaProvider _triviaProvider;

        public DidYouKnowController(ITriviaProvider triviaProvider)
        {
            _triviaProvider = triviaProvider;
        }

        public ActionResult More(int id)
        {
            return View(_triviaProvider.GetTrivia(id));
        }
    }
}
