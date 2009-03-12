using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using Modular.Common.Services;
using Spark.Modules;

namespace Modular.Games.WebPackage.Blocks
{
    public class HighScoreBlock : BlockBase
    {
        private readonly IHighScoreRepository _highScoreRepository;

        public HighScoreBlock(IHighScoreRepository highScoreRepository)
        {
            _highScoreRepository = highScoreRepository;
        }

        public override void RenderBlock()
        {
            Html.RenderPartial(@"Games\Home\Scores", _highScoreRepository.ListScores().Take(3));
        }

    }
}
