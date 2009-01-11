using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using Modular.Flair.WebPackage.Services;
using Spark.Modules;

namespace Modular.Flair.WebPackage.Blocks
{
    public class DidYouKnowBlock : IBlock
    {
        private readonly ITriviaProvider _triviaProvider;

        public DidYouKnowBlock(ITriviaProvider triviaProvider)
        {
            _triviaProvider = triviaProvider;
        }

        public HtmlHelper Html { get; set; }

        public void RenderBlock()
        {
            Html.RenderPartial(@"Flair\DidYouKnow\Block", _triviaProvider.GetRandomTrivia());
        }
    }
}
