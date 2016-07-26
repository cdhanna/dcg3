using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DCG.Framework.Physics.Bodies;
using DCG.Framework.PrimtiveBatch;
using Microsoft.Xna.Framework;

namespace DCG3
{
    public class Box
    {
        public Vector3 Position { get { return Body.Position; } set { Body.Position = value; } }
        public BoxBody Body { get; private set; }
        public Color Color { get; set; }
        public Box()
        {
            Body = new BoxBody(Vector3.Zero, Vector3.One );
            Color = Color.White;

            Body.Rotation = Quaternion.Identity;

        }

        public void Update()
        {
            
        }

        public void Draw(PrimitiveBatch pb)
        {
            pb.Cube(Position, Body.Size , Body.Rotation, Color);
        }
    }
}
