using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ThreeD.PrimtiveBatch
{
    internal class TextureAtlas
    {

        public int TextureSize { get; private set; }
        public Texture2D Texture { get { return _atlas; } }


        private Texture2D _atlas;
        private Dictionary<Texture2D, TextureAtlasIndex> _atlasLookup;
        private int _atlasMaxRowHeight;
        private Vector2 _offset;

        private GraphicsDevice _device;

        public TextureAtlas(GraphicsDevice device, int size)
        {
            TextureSize = size;
            _device = device;

            _atlas = new Texture2D(device, TextureSize, TextureSize, false, SurfaceFormat.Color);
            _atlasLookup = new Dictionary<Texture2D, TextureAtlasIndex>();

            var blankAtlasData = new Color[TextureSize * TextureSize];
            for (int i = 0; i < blankAtlasData.Length; i++)
                blankAtlasData[i] = Color.Gray;

            _atlas.SetData(0, new Rectangle(0, 0, TextureSize, TextureSize), blankAtlasData, 0, blankAtlasData.Length);
            _atlasLookup.Clear();


            _offset = Vector2.Zero;
        }

        public TextureAtlasIndex GetTexture(Texture2D texture)
        {
            if (_atlasLookup.ContainsKey(texture))
                return _atlasLookup[texture];
            throw new Exception("That texture is not in the atlas");
        }

        public TextureAtlasIndex AddTexture(Texture2D texture)
        {

            if (_atlasLookup.ContainsKey(texture))
                return _atlasLookup[texture];

            var size = new Vector2(texture.Width, texture.Height);

            if (texture.Width > _atlas.Width || texture.Height > _atlas.Height)
            {
                throw new Exception("Texture atlas isn't large enough to hold the image");
            }

            if (_offset.X + size.X > _atlas.Width)
            {
                _offset.X = 0;
                _offset.Y += _atlasMaxRowHeight;
                _atlasMaxRowHeight = 0;
            }
            if (_offset.Y + size.Y > _atlas.Height)
            {
                throw new Exception("Texture atlas out of memory. Boost memory, or use less images");
            }
            if (size.Y > _atlasMaxRowHeight)
            {
                _atlasMaxRowHeight = (int)Math.Ceiling(size.Y);
            }

            var position = new Vector2(_offset.X, _offset.Y);

            var index = new TextureAtlasIndex("whocares", position, size);


            _atlasLookup.Add(texture, index);
            var data = new Color[texture.Width * texture.Height];
            texture.GetData(data);

            _atlas.SetData(0,
                new Rectangle((int)index.Position.X, (int)index.Position.Y, (int)index.Size.X, (int)index.Size.Y),
                data, 0, data.Length);

            _offset = new Vector2(_offset.X + size.X, _offset.Y);

            return index;
        }

    }

    internal class VerticiesAndIndicies
    {
        public List<VertexPositionColorNormalTexture> Verticies { get; set; }
        public List<short> Indices { get; set; }

        public VerticiesAndIndicies()
        {
            Verticies = new List<VertexPositionColorNormalTexture>();
            Indices = new List<short>();

        }

        public VerticiesAndIndicies(List<VertexPositionColorNormalTexture> verts, List<short> inds)
        {
            Verticies = verts;
            Indices = inds;
        }
    }

    internal struct TextureAtlasIndex
    {
        public Vector2 Position;
        public Vector2 Size;
        public string Name;

        public TextureAtlasIndex(string name, Vector2 pos, Vector2 size)
        {
            Name = name;
            Position = pos;
            Size = size;
        }
    }

}
