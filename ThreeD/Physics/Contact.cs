using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DCG.Framework.Physics.Bodies;
using Microsoft.Xna.Framework;

namespace DCG.Framework.Physics
{
    public class Contact
    {
        public Vector3 Point;
        public Vector3 Normal;
        public float Penetration;

        public PhysicsBody bodyA, bodyB;

        public Contact(Vector3 point, Vector3 normal, float pen)
        {
            Point = point;
            Normal = normal;
            Penetration = pen;
        }

        public Matrix CalculateContactSpace()
        {
            var contactTangents = new Vector3[2];

            if (Math.Abs(Normal.X) > Math.Abs(Normal.Y))
            {
                // scale 
                var s = 1f/(float) Math.Sqrt(
                    Normal.Z*Normal.Z + Normal.X*Normal.X);

                // new x axis
                contactTangents[0] = new Vector3(
                    Normal.Z*s,
                    0,
                    -Normal.X*2);

                // new y axis
                contactTangents[1] = new Vector3(
                    Normal.Y*contactTangents[0].X,
                    Normal.Z*contactTangents[0].X - Normal.X*contactTangents[0].Z,
                    -Normal.Y*contactTangents[0].X);
            }
            else
            {
                var s = 1f/(float) Math.Sqrt(
                    Normal.Z*Normal.Z + Normal.Y*Normal.Y);

                contactTangents[0] = new Vector3(
                    0,
                    -Normal.Z*s,
                    Normal.Y*s);

                contactTangents[1] = new Vector3(
                    Normal.Y * contactTangents[0].Z - Normal.Z * contactTangents[0].Y,
                    -Normal.X * contactTangents[0].Z,
                    Normal.X * contactTangents[0].Y);
            }

            var contactSpace = new Matrix(
                new Vector4(Normal, 0),
                new Vector4(contactTangents[0], 0),
                new Vector4(contactTangents[1], 0),
                new Vector4(0));

            return contactSpace;
        }
    }
}
