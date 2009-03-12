using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Modular.Common.Services;

namespace Modular.Games.WebPackage.Models
{
    public class HangmanGame
    {
        private readonly IList<Tile> _alphabet;
        private readonly IList<Tile> _solution;

        public HangmanGame(string solution)
        {
            Answer = solution;
            Id = Guid.NewGuid();

            _alphabet = "abcdefghijklmnopqrstuvwxyz ".ToArray()
                .Select(letter => new Tile { Letter = letter.ToString() })
                .ToArray();
            
            _alphabet.Single(t => t.Letter == " ").State = TileState.Hit;

            _solution = solution.ToArray()
                .Select(letter => _alphabet.FirstOrDefault(t => t.Letter == letter.ToString()))
                .ToArray();
        }

        public Guid Id { get; set; }
        public int Moves { get; set; }
        public bool Victory { get; set; }

        public void Guess(string letter)
        {
            if (!Victory)
                Moves += 1;

            var guess = _alphabet.Single(t => t.Letter == letter);
            if (_solution.Contains(guess))
                guess.State = TileState.Hit;
            else
                guess.State = TileState.Miss;

            if (_solution.All(t=>t.State == TileState.Hit))
                Victory = true;
        }

        public IEnumerable<IEnumerable<Tile>> Rows
        {
            get { return new[] { _alphabet.Take(13), _alphabet.Skip(13).Take(13) }; }
        }
        public IEnumerable<Tile> Solution
        {
            get { return _solution; }
        }

        public string Answer { get; set; }

        public class Tile
        {
            public string Letter { get; set; }
            public TileState State { get; set; }
        }

        public enum TileState
        {
            Unused,
            Miss,
            Hit
        }
    }
}
