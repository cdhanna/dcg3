using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using ThreeD;
using ThreeD.PrimtiveBatch;

namespace DCG3.GameLogic
{
    class Level
    {
        public List<Cube> Cubes { get; set; }
        public Vector3 PlayerStart { get; set; }
        public Vector3 CameraStart { get; set; }

        public Level()
        {
            Cubes = new List<Cube>();
            PlayerStart = Vector3.Zero;
            CameraStart = new Vector3(0, 5, -5);
        }

        public void Draw(IPrimitiveBatch pBatch)
        {
            Cubes.ForEach(c => c.Draw(pBatch));

        }
    }
}
