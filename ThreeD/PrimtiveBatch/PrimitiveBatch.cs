using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Linq;
using System.Net.Mime;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ThreeD.PrimtiveBatch;

namespace ThreeD.PrimtiveBatch
{

    public class PrimitiveBatch : IPrimitiveBatch
    {
        private readonly GraphicsDevice _device;
        private readonly Texture2D _pixel;

        private readonly SpriteBatch _sb;
        private readonly BasicEffect _effect;

        private readonly Matrix _projectionMatrix;
        private readonly Matrix _worldMatrix;

        private readonly BatchCollection _batchColl;
        private readonly TextureAtlas _atlas;

        public bool HasBegun { get; private set; }


        public int BatchCount { get { return _batchColl.GetAll().Count; } }
        public int TotalVertexCount { get; private set; }

        public PrimitiveBatch(GraphicsDevice device)
        {
            // grab graphics device info
            _device = device;
            _sb = new SpriteBatch(device);

            // create a utility pixel texture to use if no texture is supplied
            _pixel = new Texture2D(device, 1, 1, false, SurfaceFormat.Color);
            _pixel.SetData(new Color[] { Color.White });

            // create a texture atlas and a batch collection
            _atlas = new TextureAtlas(device, 1024);
            _atlas.AddTexture(_pixel);
            _batchColl = new BatchCollection();


            // configure basic projection and worl matrix.
            _projectionMatrix = Matrix.CreatePerspectiveFieldOfView(
                MathHelper.ToRadians(45f),
                device.DisplayMode.AspectRatio, 1f, 10000f);
            _worldMatrix = Matrix.CreateWorld(Vector3.Zero, Vector3.Forward, Vector3.Up);

            // make a basic effect that accepts our inputs
            _effect = new BasicEffect(device);
            _effect.Alpha = 1.0f;
            _effect.VertexColorEnabled = true;
            _effect.TextureEnabled = true;
            //_effect.LightingEnabled = true;

            //_effect.AmbientLightColor = new Vector3(.5f, .5f, .5f);
            _effect.EnableDefaultLighting();

            HasBegun = false;
        }



        public void Begin()
        {
            if (HasBegun)
                throw new Exception("The batch has already begun. It must be ended before another can begin");
            _batchColl.Clear();
            HasBegun = true;
            TotalVertexCount = 0;
        }

        public void Cube(Vector3 position, Vector3 size, Rotation rotation)
        {
            Cube(position, size, rotation, Color.White);   
        }

        public void Cube(Vector3 position, Vector3 size, Rotation rotation, Color color)
        {
            Cube(position, size, rotation, color, null, Vector2.One, SamplerState.LinearClamp, TextureStyle.PerQuad);
        }

        public void Cube(Vector3 position, Vector3 size, Rotation rotation, Texture2D texture,
            TextureStyle textureStyle = TextureStyle.PerQuad)
        {
            Cube(position, size, rotation, Color.White, texture, Vector2.One, SamplerState.LinearClamp, textureStyle);
        }

        public void Cube(Vector3 position, Vector3 size, Rotation rotation, Texture2D texture, Vector2 textureScale,
            SamplerState samplerState, TextureStyle textureStyle = TextureStyle.PerQuad)
        {
            Cube(position, size, rotation, Color.White, texture, textureScale, samplerState, textureStyle);
        }

        public void Cube(Vector3 position, Vector3 size, Rotation rotation, Color color, Texture2D texture,
            Vector2 textureScale, SamplerState samplerState, TextureStyle textureStyle)
        {

            if (!HasBegun)
                throw new Exception("The batch has not begun, and cannot be drawn to");

            // are we using a texture atlas ?
            var cube = GetBaseCube(position, size, rotation);
            cube.Verticies = cube.Verticies.Color(color);

            if (texture == null)
                texture = _pixel;

            if (samplerState == null)
                samplerState = SamplerState.LinearClamp;

            if (textureStyle == null)
                textureStyle = TextureStyle.Wrap;

            Batch batch = null;
            if (samplerState.Equals(SamplerState.LinearClamp))
            {
                // using texture atlas
                batch = GetBatch(_atlas.Texture, SamplerState.LinearClamp);
                ApplyTexture(cube.Verticies, textureStyle, textureScale, texture, true);
            }
            else if (samplerState.Equals(SamplerState.LinearWrap))
            {
                // using a custom texture 
                batch = GetBatch(texture, SamplerState.LinearWrap);
                ApplyTexture(cube.Verticies, textureStyle, textureScale, texture, false);
            }
            else throw new Exception("only linear clamp and wrap are supported sampler states");


            batch.Add(cube);

        }


        public void Flush(Matrix viewMatrix)
        {
            if (!HasBegun)
                throw new Exception("The batch has not been started, and cannot be drawn");

            _effect.Projection = _projectionMatrix;
            _effect.View = viewMatrix;
            _effect.World = _worldMatrix;
            _device.DepthStencilState = DepthStencilState.Default;
            _device.RasterizerState = RasterizerState.CullCounterClockwise;


            _batchColl.GetAll().ForEach(batch =>
            {

                _effect.Texture = batch.Config.Texture;

                if (batch.Config.SamplerState == SamplerState.LinearClamp)
                {
                    _device.SamplerStates[0] = batch.Config.SamplerState;
                }
                else
                {
                    _device.SamplerStates[0] = SamplerState.PointWrap;
                }

                var vBuffer = new VertexBuffer(_device, typeof(CustomVertexDeclaration), batch.Verticies.Count, BufferUsage.WriteOnly);
                vBuffer.SetData(batch.Verticies.ToArray());

                var iBuffer = new IndexBuffer(_device, typeof(short), batch.Indicies.Count, BufferUsage.WriteOnly);
                iBuffer.SetData(batch.Indicies.ToArray());

                _device.SetVertexBuffer(vBuffer);
                _device.Indices = iBuffer;

                TotalVertexCount += vBuffer.VertexCount;

                foreach (EffectPass pass in _effect.CurrentTechnique.Passes)
                {
                    pass.Apply(); // sends pass to gfx. 
                    _device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, iBuffer.IndexCount / 3);
                }
            });

            //_sb.Begin();
            //_sb.Draw(_atlas.Texture, new Rectangle(0, 0, 100, 100), new Color(1f, 1f, 1f, .5f));
            //_sb.End();

            HasBegun = false;
        }

        private Batch GetBatch(Texture2D texture, SamplerState state)
        {
            var conf = new BatchConfig(texture, PrimitiveType.TriangleList, state, BlendState.NonPremultiplied);
            return _batchColl.Get(conf);
        }

        private void ApplyTexture(List<CustomVertexDeclaration> verts, TextureStyle style, Vector2 scale, Texture2D texture, bool inAtlas)
        {
            var transformUV = new Func<Vector2, Vector2>(v => v * scale); // this is the 'wrap' version, where the atlas isn't being used.

            var texIndex = new TextureAtlasIndex(); // start as undefined.
            if (inAtlas)
            {
                texIndex = _atlas.AddTexture(texture);

                // set the func to transform the UV to the atlas coordinate system.
                var atlasSize = new Vector2(_atlas.TextureSize, _atlas.TextureSize);
                var ratio = texIndex.Size / atlasSize;
                transformUV = new Func<Vector2, Vector2>(v => (v * scale) * ratio + texIndex.Position / atlasSize);
            }

            for (int s = 0; s < 6; s++)
            {
                var start = s * 4;
                for (int i = 0; i < 4; i++)
                {
                    var x = (i == 1 || i == 2 ? s : (s + 1)) / 6f;
                    var y = i == 2 || i == 3 ? 1 : 0;

                    if (style == TextureStyle.PerQuad)
                    {
                        x = (i == 1 || i == 2 ? 0 : 1);
                    }

                    var vert = verts[i + start];
                    verts[i + start] = new CustomVertexDeclaration(
                        vert.Position,
                        vert.Color,
                        transformUV(new Vector2(x, y)),
                        vert.Normal);
                }
            }
        }

        private VerticiesAndIndicies GetBaseCube(Vector3 position, Vector3 size, Rotation rotation)
        {
            var cube = UnitCube;
            var axis = rotation.Axis;
            //waxis.Normalize();


            var verts = cube.Verticies
                .Scale(size)
                .Rotate(axis, rotation.Radians)
                .Translate(position);

            return new VerticiesAndIndicies(verts, cube.Indices);
        }

        private static readonly VerticiesAndIndicies UnitQuad =
            new VerticiesAndIndicies(new CustomVertexDeclaration[]
            {
                new CustomVertexDeclaration(new Vector3(0, 0, 0), Color.White, new Vector2(0, 0), new Vector3(0,0, -1)), 
                new CustomVertexDeclaration(new Vector3(1, 0, 0), Color.White, new Vector2(0, 0), new Vector3(0,0, -1)), 
                new CustomVertexDeclaration(new Vector3(1, 1, 0), Color.White, new Vector2(0, 0), new Vector3(0,0, -1)), 
                new CustomVertexDeclaration(new Vector3(0, 1, 0), Color.White, new Vector2(0, 0), new Vector3(0,0, -1)), 
            }.ToList().Translate(new Vector3(-.5f, -.5f, 0)),
            new short[]
            {
                0, 1, 2, 0, 2, 3
            }.ToList());


        private static readonly VerticiesAndIndicies UnitCube =
            new VerticiesAndIndicies(new CustomVertexDeclaration[]
            {
                
            }.ToList()
            .Concat(UnitQuad.Verticies.Rotate(Vector3.UnitY, MathHelper.PiOver2).Rotate(Vector3.UnitX, MathHelper.Pi).Translate(Vector3.UnitX * -.5f)) // left
            .Concat(UnitQuad.Verticies.Rotate(Vector3.UnitX, MathHelper.Pi).Translate(Vector3.UnitZ * .5f)) // back
            .Concat(UnitQuad.Verticies.Rotate(Vector3.UnitY, -MathHelper.PiOver2).Rotate(Vector3.UnitX, MathHelper.Pi).Translate(Vector3.UnitX * .5f)) // right

            .Concat(UnitQuad.Verticies.Rotate(Vector3.UnitZ, MathHelper.Pi).Translate(Vector3.UnitZ * -.5f)) // front
            .Concat(UnitQuad.Verticies.Rotate(Vector3.UnitX, -MathHelper.PiOver2).Translate(Vector3.UnitY * -.5f)) // bot
            .Concat(UnitQuad.Verticies.Rotate(Vector3.UnitX, MathHelper.PiOver2).Translate(Vector3.UnitY * .5f)) // top


            ,
            new short[]
            {
                0, 1, 2, 0, 2, 3,
                4, 5, 6, 4, 6, 7,
                8, 9, 10, 8, 10, 11,
                12, 13, 14, 12, 14, 15,
                16, 17, 18, 16, 18, 19,
                20, 21, 22, 20, 22, 23
            }.ToList());


        //private List<CustomVertexDeclaration> UnitCube()
        //{
        //    var cube = new List<CustomVertexDeclaration>();

        //    return cube;
        //}

    }
}
