using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace DCG3.GameLogic
{
    class Cube
    {
        public Vector3 Position { get; set; }
        public Color Color { get; set; }
        public Vector3 Size { get; set; }

        public void Draw(IPrimitiveBatch pBatch)
        {
            pBatch.Draw(Position, Size.X, Size.Y, Size.Z, Color);
        }
    }
}
