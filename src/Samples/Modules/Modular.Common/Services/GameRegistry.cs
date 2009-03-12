using System.Collections.Generic;
using System.Linq;
using Spark.Modules;

namespace Modular.Common.Services
{
    public interface IGameRegistry : IService
    {
        void AddGame(string name, object playLinkValues);
        IEnumerable<GameDescriptor> ListGames();
    }

    class GameRegistry : IGameRegistry
    {
        private readonly IDictionary<string, GameDescriptor> _games = new Dictionary<string, GameDescriptor>();

        public void AddGame(string name, object playLinkValues)
        {
            _games.Add(name, new GameDescriptor { Name = name, PlayLinkValues = playLinkValues });
        }

        public IEnumerable<GameDescriptor> ListGames()
        {
            return _games.Values.ToArray();
        }
    }
}

