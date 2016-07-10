using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace DCG3.GameLogic
{
    class SimpleCamera
    {
        public Vector3 Position { get; set; }
        public Vector3 Target { get; set; }

        public SimpleCamera()
        {
            Position = Vector3.Zero;
            Target = Vector3.Zero;
        }

        public Matrix GetView()
        {
            return Matrix.CreateLookAt(Position, Target, Vector3.UnitY);
        }

        public void Pan(Vector3 translate)
        {
            Position += translate;
            Target += translate;
        }

    }
}
