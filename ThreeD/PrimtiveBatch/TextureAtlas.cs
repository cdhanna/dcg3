using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DCG.Framework.PrimtiveBatch
{
    internal class TextureAtlas
    {

        /// <summary>
        /// The size of the TextureAtlas' width AND height. The TextureAtlas will always be a square image.
        /// </summary>
        public int TextureSize { get; private set; }

        /// <summary>
        /// Gets the actual Texture2D of the TextureAtlas
        /// </summary>
        public Texture2D Texture { get; private set; }


        private readonly Dictionary<Texture2D, TextureAtlasIndex> _atlasLookup;
        private int _atlasMaxRowHeight;
        private Vector2 _offset;

        public TextureAtlas(GraphicsDevice device, int size)
        {
            TextureSize = size;
            _atlasLookup = new Dictionary<Texture2D, TextureAtlasIndex>();

            // creates the atlas texture with the width&height
            Texture = new Texture2D(device, TextureSize, TextureSize, false, SurfaceFormat.Color);
            
            // writes blank data to the texture atlas. Currently, the blank data is the color Gray
            var blankAtlasData = new Color[TextureSize * TextureSize];
            for (int i = 0; i < blankAtlasData.Length; i++)
                blankAtlasData[i] = Color.Gray;

            Texture.SetData(0, new Rectangle(0, 0, TextureSize, TextureSize), blankAtlasData, 0, blankAtlasData.Length);
            _atlasLookup.Clear();


            _offset = Vector2.Zero;
        }

        /// <summary>
        /// Makes sure the given texture is in the atlas, and gets the atlasIndex of where the texture is.
        /// </summary>
        /// <param name="texture"></param>
        /// <returns></returns>
        public TextureAtlasIndex EnsureTexture(Texture2D texture)
        {

            if (_atlasLookup.ContainsKey(texture)) // do we already have this image, if so, goody!
            {
                return _atlasLookup[texture];
            }
                
            var size = new Vector2(texture.Width, texture.Height);

            // is the width or the height of the new image larger than the atlas itself, because if so, we are screwed
            if (texture.Width > Texture.Width || texture.Height > Texture.Height)
            {
                throw new Exception("Texture atlas isn't large enough to hold the image");
            }

            // we are going to put the image in the atlas at the _offset variable. 
            // we need to check if the current offset.X is far enough away from the right edge of the image.
            if (_offset.X + size.X > Texture.Width)
            {
                // make a new line of images
                _offset.X = 0;
                _offset.Y += _atlasMaxRowHeight;
                _atlasMaxRowHeight = 0;
            }

            // we need to check if we are out of y space.
            if (_offset.Y + size.Y > Texture.Height)
            {
                throw new Exception("Texture atlas out of memory. Boost memory, or use less images");
            }

            // keep track of the image that has the largest Y size in the current row. 
            // because when we need to drop down a line, we need to go down to this level
            if (size.Y > _atlasMaxRowHeight)
            {
                _atlasMaxRowHeight = (int)Math.Ceiling(size.Y);
            }

            var position = new Vector2(_offset.X, _offset.Y);
            var index = new TextureAtlasIndex(position, size);
            _atlasLookup.Add(texture, index);
            
            // actually put the iamge into the atlas
            var data = new Color[texture.Width * texture.Height];
            texture.GetData(data);
            Texture.SetData(0,
                new Rectangle((int)index.Position.X, (int)index.Position.Y, (int)index.Size.X, (int)index.Size.Y),
                data, 0, data.Length);

            // update the offset for next time.
            _offset = new Vector2(_offset.X + size.X, _offset.Y);

            return index;
        }

    }



}
