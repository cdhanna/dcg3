using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DCG3.GameLogic
{
    enum AngleType { Degree, Radian }
    enum Direction { X, Y, Z }
    class SimpleCamera
    {
        public Vector3 Position { get; set; }
        public Vector3 Target { get; set; }
        public Vector3 Up { get; set; }

        public Matrix ProjectionMatrix { get; set; }

        public SimpleCamera(GraphicsDevice device)
        {
            ProjectionMatrix = Matrix.CreatePerspectiveFieldOfView(
                MathHelper.ToRadians(45f),
                device.DisplayMode.AspectRatio, .01f, 100f);
            Position = Vector3.Zero;
            Target = Vector3.Zero;
            Up = Vector3.Up;
        }

        public Matrix GetView()
        {
            return Matrix.CreateLookAt(Position, Target, Up);
        }

        public void Pan(Vector3 translate)
        {
            Position += translate;
            Target += translate;
        }


        public void Update()
        {
            if (KeyboardHelper.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.NumPad1))
            {
                Rotate(Direction.Y, -2, AngleType.Degree);
            }
            if (KeyboardHelper.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.NumPad2))
            {
                Rotate(Direction.Y, 2, AngleType.Degree);
            }
            if (KeyboardHelper.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.NumPad4))
            {
                Rotate(Direction.X, -2, AngleType.Degree);
            }
            if (KeyboardHelper.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.NumPad5))
            {
                Rotate(Direction.X, 2, AngleType.Degree);
            }
            if (KeyboardHelper.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.NumPad7))
            {
                Rotate(Direction.Z, -2, AngleType.Degree);
            }
            if (KeyboardHelper.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.NumPad8))
            {
                Rotate(Direction.Z, 2, AngleType.Degree);
            }
        }

        public void Rotate(Direction direction, float angle, AngleType type)
        {
            Matrix rotationMatrix;
            bool isInDegrees = type == AngleType.Degree;

            float angleInRadians = isInDegrees
                ? MathHelper.ToRadians(angle)
                : angle;

            switch (direction)
            {
                case Direction.X:
                    rotationMatrix = Matrix.CreateRotationX(angleInRadians);
                    break;
                case Direction.Y:
                    rotationMatrix = Matrix.CreateRotationY(angleInRadians);
                    break;
                case Direction.Z:
                default:
                    rotationMatrix = Matrix.CreateRotationZ(angleInRadians);
                    break;
            }

            Position = Vector3.Transform(Position, rotationMatrix);
            Target = Vector3.Transform(Target, rotationMatrix);
            Up = Vector3.Transform(Up, rotationMatrix);
        }
    }
}
