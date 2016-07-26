using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DCG.Framework.Physics.Bodies;
using DCG.Framework.Physics.Colliders;
using DCG.Framework.PrimtiveBatch;
using Microsoft.Xna.Framework;

namespace DCG3
{
    class Sphere
    {
        public SphereBody Body { get; set; }
        public Color Color { get; set; }

        public Vector3 Position { get { return Body.Position;} set { Body.Position = value; } }
        public AbsCollider Collider { get { return Body.Collider; } }

        public Sphere()
        {
            Color = Color.White;
            Body = new SphereBody();
        }

        public void Update()
        {
            //Body.Update(null);

        }

        public void Draw(PrimitiveBatch pb)
        {
            pb.Sphere(Body.Position, Vector3.One, Quaternion.Identity, Color, null);
        }

    }
}
