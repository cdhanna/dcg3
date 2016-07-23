using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using DCG.Framework;
using DCG.Framework.PrimtiveBatch;
using DCG.Framework.Util;

namespace DCG3.GameLogic
{
    class Player
    {
        public Cube CubeFront { get; set; }
        public Cube CubeBack { get; set; }
        public Cube CubeRod { get; set; }


        public Vector3 Position {
            get { return CubeFront.Position; }
            set { CubeFront.Position = value; }
        }
    

        public float WheelAngle;
        public float BikeAngle;
        public float Velocity;

        public Vector3 BackPosition { get; private set; }

        public Player()
        {
            CubeFront = new Cube();
            CubeFront.Size = new Vector3(.7f, 1, .7f);

            CubeBack = new Cube();
            CubeBack.Size = Vector3.One;
            CubeBack.Size = new Vector3(.9f, 1, .7f);

            BikeAngle = 0;

            CubeRod = new Cube();
            CubeRod.Size = new Vector3(.3f, .3f, 1.5f);
            CubeRod.Color = Color.Gray;
            
        }

        public void Update(GameTime time)
        {

            var acceleration = 0f;
            var speed = .003f;

            if (KeyboardHelper.IsKeyDown(Keys.Left))
            {
                WheelAngle += .05f;
            }

            if (KeyboardHelper.IsKeyDown(Keys.Right))
            {
                WheelAngle -= .05f;
            }

            if (KeyboardHelper.IsKeyDown(Keys.Up))
            {
                acceleration += speed;
            }

            if (KeyboardHelper.IsKeyDown(Keys.Down))
            {
                acceleration -= speed;
            }

            WheelAngle -= .1f * WheelAngle;
            Velocity += acceleration;
            Velocity -= Velocity*.02f;

            BikeAngle += WheelAngle*Velocity * .6f;
            var forwards = new Vector3((float)Math.Sin(BikeAngle), 0, (float)Math.Cos(BikeAngle));
            Position += forwards * Velocity;


            CubeFront.Rotation = Quaternion.CreateFromAxisAngle(new Vector3(0, 1, 0), BikeAngle + WheelAngle);

            CubeBack.Rotation = Quaternion.CreateFromAxisAngle(new Vector3(0, 1, 0), BikeAngle);
            CubeBack.Position = CubeFront.Position - new Vector3((float)Math.Sin(BikeAngle), 0, (float)Math.Cos(BikeAngle)) * 2;

            var diff = CubeFront.Position - CubeBack.Position;
            CubeRod.Position = CubeBack.Position + diff/2;
            CubeRod.Rotation = Quaternion.CreateFromAxisAngle(new Vector3(0, 1, 0), (float)Math.Atan2(diff.X, diff.Z));

            BackPosition = CubeBack.Position - diff*2 + new Vector3(0, 4, 0);
        }

        public void Draw(IPrimitiveBatch pBatch)
        {
            CubeFront.Draw(pBatch);
            CubeBack.Draw(pBatch);
            CubeRod.Draw(pBatch);
        }

    }
}
