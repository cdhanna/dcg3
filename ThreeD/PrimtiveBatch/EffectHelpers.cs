using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DCG.Framework.PrimtiveBatch
{
    public static class EffectHelpers
    {

        public static void TrySet(this EffectParameterCollection self, string paramName, Texture paramValue)
        {
            if (self[paramName] != null)
            {
                self[paramName].SetValue(paramValue);
            }
        }
        public static void TrySet(this EffectParameterCollection self, string paramName, Matrix paramValue)
        {
            if (self[paramName] != null)
            {
                self[paramName].SetValue(paramValue);
            }
        }
        public static void TrySet(this EffectParameterCollection self, string paramName, bool paramValue)
        {
            if (self[paramName] != null)
            {
                self[paramName].SetValue(paramValue);
            }
        }
    }
}
