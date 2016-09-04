using DCG.Framework.Net;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DCG3.NetTest
{
    public class MoveInput : InputTyped<Vector3>
    {

        public MoveInput(Vector3 value) : base(value)
        {
            TypedValue = value;
        }
    }
}
