using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ThreeD;
using ThreeD.PrimtiveBatch;

namespace DCG3.GameLogic
{
    class Cube
    {
        public Vector3 Position { get; set; }
        public Color Color { get; set; }
        public Vector3 Size { get; set; }
        public Quaternion Rotation { get; set; }
        public Texture2D Texture { get; set; }
        public Vector2 UV { get; set; }
        public Vector2 UVOffset { get; set; }
        public TextureStyle TextureStyle { get; set; }

        public Cube()
        {
            Rotation = Quaternion.Identity;
            Position = Vector3.Zero;
            Color = Color.White;
            Size = Vector3.One;
            UV = Vector2.One;
            UVOffset = Vector2.Zero;
            TextureStyle = TextureStyle.PerQuad;
        }

        public void Draw(IPrimitiveBatch pBatch)
        {
            //pBatch.Cube(Position, Size, Rotation, Color);
             pBatch.Cube(Position, Size, Rotation, Color, Texture, UV, UVOffset, SamplerState.LinearWrap, TextureStyle);;
            // pBatch.Draw(Position, Size, Quaternion.Identity,  Texture);
        }
    }
}
