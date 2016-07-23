using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace DCG.Framework.Physics
{
    public class Contact
    {
        public Vector3 Point;
        public Vector3 Normal;
        public float Penetration;


        public Contact(Vector3 point, Vector3 normal, float pen)
        {
            Point = point;
            Normal = normal;
            Penetration = pen;
        }
    }
}
