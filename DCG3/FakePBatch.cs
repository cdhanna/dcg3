using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DCG3
{
    class FakePBatch : IPrimitiveBatch
    {
        public void Begin(Microsoft.Xna.Framework.Graphics.GraphicsDevice device)
        {
            throw new NotImplementedException();
        }

        public void Draw(Microsoft.Xna.Framework.Vector3 position, float width, float height, float depth, Microsoft.Xna.Framework.Color color)
        {
            throw new NotImplementedException();
        }

        public void Flush(Microsoft.Xna.Framework.Matrix viewMatrix)
        {
            throw new NotImplementedException();
        }
    }
}
