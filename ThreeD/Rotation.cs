using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace DCG.Framework
{
    public struct Rotation
    {
        public Vector3 Axis;
        public float Radians;

        public Rotation(float radians)
        {
            Axis = Vector3.Zero;
            Radians = radians;
        }

        public Rotation(Vector3 axis, float radians)
        {
            Axis = axis;
            Radians = radians;
        }

        public static readonly Rotation None = new Rotation(0f);
    }
}
