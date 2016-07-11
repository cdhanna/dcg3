using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ThreeD.PrimtiveBatch
{
    public interface IPrimitiveBatch
    {
        void Begin();

        void Cube(Vector3 position, Vector3 size, Rotation rotation);

        void Cube(Vector3 position, Vector3 size, Rotation rotation, Color color);

        void Cube(Vector3 position, Vector3 size, Rotation rotation, Texture2D texture, TextureStyle textureStyle=TextureStyle.PerQuad);

        void Cube(Vector3 position, Vector3 size, Rotation rotation, Texture2D texture, Vector2 textureScale, SamplerState samplerState, TextureStyle textureStyle = TextureStyle.PerQuad);

        void Cube(Vector3 position, Vector3 size, Rotation rotation, Color color, Texture2D texture, Vector2 textureScale, Vector2 textureOffset, SamplerState samplerState, TextureStyle textureStyle = TextureStyle.PerQuad);

        void Flush(Matrix viewMatrix, Matrix projectionMatrix);
    }

    public enum TextureStyle
    {
        Wrap,
        PerQuad
    }

   
}
