using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace DCG.Framework.PrimtiveBatch
{

    class BaseLight
    {
        public Color Color;
        public ShadowData Shadow;
    }

    class ShadowData
    {
        public Matrix Projection;
        public int ShadowMapResolution = 1024;

        public void SetAsOrtho()
        {
            Projection = Matrix.CreateOrthographic(40, 40, -40, 40); // far plane? TODO
        }
    }

    class DirectionalLight : BaseLight
    {
        public Vector3 Direction;

        public DirectionalLight(Vector3 direction, Color color, bool shadowEnabled=false)
        {
            Direction = direction;
            Color = color;

            if (shadowEnabled)
            {
                Shadow = new ShadowData();
                Shadow.SetAsOrtho();

            }
        }

        public DirectionalLight(Vector3 direction) : this(direction, Color.White, false)
        {
            
        }
    }


    //struct DirectionalLight
    //{
    //    public Vector3 Direction;
    //    public Color Color;


    //    public DirectionalLight(Vector3 dir, Color color)
    //    {
    //        Direction = dir;
    //        Color = color;
    //    }

    //    public DirectionalLight(Vector3 dir) : this(dir, Color.White)
    //    { }
    //}

    class PointLight : BaseLight
    {
        public Vector3 Position;
        public float Radius;
        public float Intensity;

        public PointLight(Vector3 position, float radius, float intensity, Color color)
        {
            Position = position;
            Radius = radius;
            Intensity = intensity;
            Color = color;
        }
    }

    //struct PointLight
    //{
    //    public Vector3 Position;
    //    public float Radius;
    //    public float Intensity;
    //    public Color Color;

    //    public PointLight(Vector3 position, float radius, float intensity, Color color)
    //    {
    //        Position = position;
    //        Radius = radius;
    //        Intensity = intensity;
    //        Color = color;
    //    }
    //}


}
