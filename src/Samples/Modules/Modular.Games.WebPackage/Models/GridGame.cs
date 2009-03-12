using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Modular.Games.WebPackage.Models
{
    public class GridGame
    {
        public GridGame()
        {
            Id = Guid.NewGuid();
            Cells = new bool[9];
        }

        static Random _random = new Random();

        public void Randomize()
        {
            for (int index = 0; index != Cells.Length; ++index)
                Cells[index] = _random.NextDouble() > .5;
        }

        public bool[] Cells { get; set; }
        public int Moves { get; set; }
        public bool Victory { get; set; }
        public Guid Id { get; set; }
    }
}
