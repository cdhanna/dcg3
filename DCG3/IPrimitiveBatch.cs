using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace DCG3
{
    interface IPrimitiveBatch
    {
        void Begin();
        void Draw(Vector3 position, float width, float height, float depth, Color color);
        void Flush(Matrix viewMatrix);
    }
}
