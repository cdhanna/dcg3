using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DCG.Framework.PrimtiveBatch
{
    public class RenderArgs
    {
        public Vector3 Position;
        public Vector3 Size;
        public Quaternion Rotation;
        public Color Color;
        
        public Texture2D ColorMap;
        public Texture2D NormalMap;
        
        public Vector2 TextureScale;
        public Vector2 TextureOffset;
        
        public SamplerState SamplerState;

        public RenderArgs(Vector3 position, Vector3 size, Quaternion rotation, Color color, 
            Texture2D colorMap, Texture2D normalMap,
            Vector2 textureScale, Vector2 textureOffset,
            SamplerState sampler)
        {
            Position = position;
            Size = size;
            Rotation = rotation;
            Color = color;
            ColorMap = colorMap;
            NormalMap = normalMap;
            TextureScale = textureScale;
            TextureOffset = textureOffset;
            SamplerState = sampler;
        }

        public RenderArgs()
            : this(
                Vector3.Zero, Vector3.One, Quaternion.Identity, Color.White, 
                null, null,
                Vector2.One, Vector2.Zero,
                SamplerState.LinearWrap)
        {
            // the defaults have been set. 
        }


    }
}
