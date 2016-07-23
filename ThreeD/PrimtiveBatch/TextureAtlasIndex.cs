using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace DCG.Framework.PrimtiveBatch
{
    /// <summary>
    /// The TextureAtlasIndex holds the position and size of a texture inside the texture atlas.
    /// The vectors are in terms of the texture atlas space.
    /// </summary>
    internal struct TextureAtlasIndex
    {
        /// <summary>
        /// The position of the top-left of the image stored inside the texture atlas
        /// </summary>
        public Vector2 Position;

        /// <summary>
        /// The size of the image stored in the texture atlas
        /// </summary>
        public Vector2 Size;

        public TextureAtlasIndex(Vector2 pos, Vector2 size)
        {
            Position = pos;
            Size = size;
        }
    }
}
