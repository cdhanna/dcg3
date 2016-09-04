using DCG.Framework.Net;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DCG3.NetTest
{
    class SomeNetState : NetState
    {
        public Vector3 PlayerPosition { get; set; }
        public Vector3 PlayerVelocity { get; set; }
    }



}
