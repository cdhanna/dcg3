using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Input;

namespace DCG3.GameLogic
{
    class KeyboardHelper
    {

        private KeyboardState _old, _new;

        public void Update()
        {
            _old = _new;
            _new = Keyboard.GetState();
        }

        public bool IsKeyDown(Keys k)
        {
            return _new.IsKeyDown(k);
        }

        public bool IsNewKeydown(Keys k)
        {
            return _new.IsKeyDown(k) && _old.IsKeyUp(k);
        }

    }
}
