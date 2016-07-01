using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ThreeD
{
    public interface IPrimitiveBatch
    {
        void Begin();
        void Draw(Vector3 position, float width, float height, float depth, Color color);
        void Draw(Vector3 position, float width, float height, float depth, Color[] color);
        void Flush(Matrix viewMatrix);
    }
}
