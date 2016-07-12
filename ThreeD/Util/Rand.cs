using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace ThreeD.Util
{
    public class Rand
    {
        public Random InnerRandom { get; set; }

        public Rand()
        {
            InnerRandom = new Random();
        }

        public Rand(int seed)
        {
            InnerRandom = new Random(seed);
        }

        public bool RandomCheck()
        {
            return InnerRandom.Next() % 2 == 0;
        }

        public float RandomFloat(float min, float max)
        {
            var x = min + (InnerRandom.NextDouble() * (max - min));
            return (float)x;
        }

        public Vector3 RandomUnit()
        {
            var theta = RandomFloat(0, MathHelper.TwoPi);
            var z = RandomFloat(-1, 1);

            var z2 = Math.Sqrt(1 - z * z);
            var v = new Vector3((float)(z2 * Math.Cos(theta)), (float)(z2 * Math.Sign(theta)), z);
            v.Normalize();
            return v;
        }
        

    }
}
