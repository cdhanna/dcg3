using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using ThreeD;
using ThreeD.PrimtiveBatch;

namespace DCG3.GameLogic
{
    class Player
    {
        public Cube Cube { get; set; }

        public Vector3 Position {
            get { return Cube.Position; }
            set { Cube.Position = value; }
        }
        public Vector3 Velocity;

        public Player()
        {
            Cube = new Cube();
            Cube.Color = Color.Green;
            Cube.Size = Vector3.One;
        }

        public void Update(GameTime time)
        {

            var acceleration = Vector3.Zero;
            var speed = .1f;

            if (KeyboardHelper.IsKeyDown(Keys.Left))
            {
                acceleration += Vector3.UnitX*speed;
            }

            if (KeyboardHelper.IsKeyDown(Keys.Right))
            {
                acceleration -= Vector3.UnitX*speed;
            }

            if (KeyboardHelper.IsKeyDown(Keys.Up))
            {
                acceleration += Vector3.UnitZ*speed;
            }

            if (KeyboardHelper.IsKeyDown(Keys.Down))
            {
                acceleration -= Vector3.UnitZ*speed;
            }

            Velocity += acceleration;
            Velocity -= Velocity*.2f;

            Position += Velocity;

        }

        public void Draw(IPrimitiveBatch pBatch)
        {
            Cube.Draw(pBatch);
        }

    }
}
