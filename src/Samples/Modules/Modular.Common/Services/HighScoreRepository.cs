using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Spark.Modules;

namespace Modular.Common.Services
{
    public interface IHighScoreRepository : IService
    {
        void AddHighScore(DateTime victoryUtc, string name, int score, object gameState, object reviewLinkValues);
        IEnumerable<HighScoreEntry> ListScores();
    }

    public class HighScoreRepository : IHighScoreRepository
    {
        private IEnumerable<HighScoreEntry> _entries = new HighScoreEntry[0];

        public void AddHighScore(DateTime victoryUtc, string name, int score, object gameState, object reviewLinkValues)
        {
            var entries = new List<HighScoreEntry>
                              {
                                  new HighScoreEntry
                                      {
                                          VictoryUrc = victoryUtc,
                                          Name = name,
                                          Score = score,
                                          GameState = gameState,
                                          ReviewLinkValues = reviewLinkValues
                                      }
                              };
            entries.AddRange(_entries);
            _entries = entries.Take(50);
        }

        public IEnumerable<HighScoreEntry> ListScores()
        {
            return _entries;
        }
    }

    public class HighScoreEntry
    {
        public DateTime VictoryUrc { get; set; }
        public string Name { get; set; }
        public int Score { get; set; }
        public object GameState { get; set; }
        public object ReviewLinkValues { get; set; }
    }
}
