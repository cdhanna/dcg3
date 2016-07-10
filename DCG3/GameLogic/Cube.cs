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
        public Rotation Rotation { get; set; }
        public Texture2D Texture { get; set; }
        public Vector2 UV { get; set; }
        public TextureStyle TextureStyle { get; set; }

        public Cube()
        {
            Position = Vector3.Zero;
            Color = Color.White;
            Size = Vector3.One;
            UV = Vector2.One;
            TextureStyle = TextureStyle.PerQuad;
        }

        public void Draw(IPrimitiveBatch pBatch)
        {
            pBatch.Cube(Position, Size, Rotation, Color, Texture, UV, UV == Vector2.One ? SamplerState.LinearClamp : SamplerState.LinearWrap, TextureStyle);;
           // pBatch.Draw(Position, Size, Quaternion.Identity,  Texture);
        }
    }
}
