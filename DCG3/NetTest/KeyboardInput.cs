using DCG.Framework.Net;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DCG3.NetTest
{
    class KeyboardInput : KeyboardInputGenerator
    {
        public override void CheckForInputs(List<Input> inputs)
        {
            
            if (IsKeyDown(Keys.Up))
            {
                inputs.Add(new MoveInput(Vector3.Up));
            }
            if (IsKeyDown(Keys.Down))
            {
                inputs.Add(new MoveInput(Vector3.Down));
            }
            if (IsKeyDown(Keys.Left))
            {
                inputs.Add(new MoveInput(Vector3.Left));
            }
            if (IsKeyDown(Keys.Right))
            {
                inputs.Add(new MoveInput(Vector3.Right));
            }
        }
    }
}
