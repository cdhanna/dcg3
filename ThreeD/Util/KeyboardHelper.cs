using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Input;

namespace DCG.Framework.Util
{
    public class KeyboardHelper
    {

        private static KeyboardState _old, _new;

        private KeyboardHelper() { }

        public static void Update()
        {
            _old = _new;
            _new = Keyboard.GetState();
        }

        public static bool IsKeyDown(Keys k)
        {
            return _new.IsKeyDown(k);
        }

        public static bool IsNewKeydown(Keys k)
        {
            return _new.IsKeyDown(k) && _old.IsKeyUp(k);
        }

    }
}
