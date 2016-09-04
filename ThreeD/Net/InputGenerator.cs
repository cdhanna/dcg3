using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DCG.Framework.Net
{
    public abstract class InputGenerator
    {

        public abstract List<Input> CheckForInput();
    }

    public abstract class KeyboardInputGenerator : InputGenerator
    {

        private KeyboardState _old, _curr;

        public KeyboardInputGenerator()
        {

        }

        public override List<Input> CheckForInput()
        {
            _curr = Keyboard.GetState();

            var inputs = new List<Input>();
            CheckForInputs(inputs);

            _old = _curr;

            return inputs;
        }

        protected bool IsKeyDown(Keys k)
        {
            return _curr.IsKeyDown(k);
        }
        protected bool IsKeyDownNew(Keys k)
        {
            return _curr.IsKeyDown(k) && !_old.IsKeyDown(k);
        }
        protected bool IsKeyUp(Keys k)
        {
            return _curr.IsKeyUp(k);
        }
        protected bool IsKeyUpNew(Keys k)
        {
            return _curr.IsKeyUp(k) && !_old.IsKeyUp(k);
        }

        public abstract void CheckForInputs(List<Input> inputs);
    }
}
