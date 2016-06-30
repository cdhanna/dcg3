using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DCG3.GameLogic
{
    class Level
    {
        public List<Cube> Cubes { get; set; }


        public void Draw(IPrimitiveBatch pBatch)
        {
            Cubes.ForEach(c => c.Draw(pBatch));
        }
    }
}
