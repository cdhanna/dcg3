using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace DCG.Framework.PrimtiveBatch
{
    struct DirectionalLight
    {
        public Vector3 Direction;
        public Color Color;

        public DirectionalLight(Vector3 dir, Color color)
        {
            Direction = dir;
            Color = color;
        }

        public DirectionalLight(Vector3 dir) : this(dir, Color.White)
        { }
    }

    struct PointLight
    {
        public Vector3 Position;
        public float Radius;
        public float Intensity;
        public Color Color;

        public PointLight(Vector3 position, float radius, float intensity, Color color)
        {
            Position = position;
            Radius = radius;
            Intensity = intensity;
            Color = color;
        }
    }

    //struct LightData
    //{
    //    public Vector3 Position;
    //    public Color Color;
    //}
}
