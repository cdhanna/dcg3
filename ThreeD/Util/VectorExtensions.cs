﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace DCG.Framework.Util
{
    public static class VectorExtensions
    {
        public static Vector3 Normal(this Vector3 self)
        {
            return self/self.Length();
        }
    }
}