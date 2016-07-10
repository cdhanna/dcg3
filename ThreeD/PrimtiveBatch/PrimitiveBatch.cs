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

        // the _batchVBOTable and _batchIBOTable are tables that point that hold an ongoing buffer for a given batch config, across begin/flush cycles.
        private Dictionary<BatchConfig, DynamicVertexBuffer> _batchVBOTable;
        private Dictionary<BatchConfig, DynamicIndexBuffer> _batchIBOTable; 

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

            // create buffer tables
            _batchIBOTable = new Dictionary<BatchConfig, DynamicIndexBuffer>();
            _batchVBOTable = new Dictionary<BatchConfig, DynamicVertexBuffer>();


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
            _effect.PreferPerPixelLighting = true;
            //_effect.LightingEnabled = true;

            //_effect.AmbientLightColor = new Vector3(.5f, .5f, .5f);
            _effect.EnableDefaultLighting();

            HasBegun = false;
        }



        public void Begin()
        {
            if (HasBegun)
            {
                throw new Exception("The batch has already begun. It must be ended before another can begin");
            }
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
            {
                throw new Exception("The batch has not begun, and cannot be drawn to");
            }

            // are we using a texture atlas ?
            //var cube = GetBaseCube(position, size, rotation, color);

            if (texture == null)
            {
                texture = _pixel;
            }

            if (samplerState == null)
            {
                samplerState = SamplerState.LinearClamp;
            }

            Batch batch = null;
            var inAtlas = false;

            if (samplerState.Equals(SamplerState.LinearClamp))
            {
                // using texture atlas
                batch = GetBatch(_atlas.Texture, SamplerState.LinearClamp);
                inAtlas = true;
            }
            else if (samplerState.Equals(SamplerState.LinearWrap))
            {
                // using a custom texture 
                batch = GetBatch(texture, SamplerState.LinearWrap);
            }
            else throw new Exception("only linear clamp and wrap are supported sampler states");

            List<VertexPositionColorNormalTexture> cubeVerts = ApplyCubeDetails(UnitCube.Verticies, position, size, rotation, color, textureStyle, textureScale, texture, inAtlas);
            

            batch.Add(new VerticiesAndIndicies(cubeVerts, UnitCube.Indices));

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


            _effect.DirectionalLight2.Enabled = true;
            _effect.DirectionalLight2.Direction = new Vector3(1, 0, 0);
            _effect.DirectionalLight2.DiffuseColor = new Vector3(1, 0, 0);
            _effect.DirectionalLight2.SpecularColor = new Vector3(0, 1, 0);

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


                //var vBuffer = new VertexBuffer(_device, typeof(VertexPositionColorNormalTexture), batch.Verticies.Count, BufferUsage.None);
                //vBuffer.SetData(batch.Verticies.ToArray());

                //var iBuffer = new IndexBuffer(_device, typeof(short), batch.Indicies.Count, BufferUsage.None);
                //iBuffer.SetData(batch.Indicies.ToArray());

                var vBuffer = GetVBOForBatch(batch);
                vBuffer.SetData(batch.Verticies.ToArray());
                var iBuffer = GetIBOForBatch(batch);
                iBuffer.SetData(batch.Indicies.ToArray());

                _device.SetVertexBuffer(vBuffer);
                _device.Indices = iBuffer;

                TotalVertexCount += vBuffer.VertexCount;

                foreach (EffectPass pass in _effect.CurrentTechnique.Passes)
                {
                    pass.Apply(); // sends pass to gfx. 
                    _device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, iBuffer.IndexCount / 3);
                }

                //vBuffer.Dispose();
                //iBuffer.Dispose();
            });

            _sb.Begin();
            _sb.Draw(_atlas.Texture, new Rectangle(0, 0, 100, 100), new Color(1f, 1f, 1f, .5f));
            _sb.End();

            HasBegun = false;
        }

        private Batch GetBatch(Texture2D texture, SamplerState state)
        {
            var conf = new BatchConfig(texture, PrimitiveType.TriangleList, state, BlendState.NonPremultiplied);
            return _batchColl.Get(conf);
        }

        // get the VBO for this batch, and ensure that it is the right size.
        private DynamicVertexBuffer GetVBOForBatch(Batch batch)
        {
            var config = batch.Config;

            var create = false;
            var had = false;
            DynamicVertexBuffer vbo = null;
            if (_batchVBOTable.TryGetValue(config, out vbo))
            {
                had = true;
                if (vbo.VertexCount != batch.Verticies.Count)
                {
                    create = true;
                }
            }
            else // there wasn't even a buffer!
            {
                create = true;
            }

            if (create)
            {
                var nextVbo = new DynamicVertexBuffer(_device, typeof(VertexPositionColorNormalTexture), batch.Verticies.Count, BufferUsage.None);
                
                // if we used to have a vbo, we need to toss the old one
                if (had)
                {
                    vbo.Dispose();
                    _batchVBOTable[config] = nextVbo;
                }
                else
                {
                    _batchVBOTable.Add(config, nextVbo);
                }

                vbo = nextVbo;
            }

            return vbo;
        }

        // get the IBO for this batch, and ensure that it is the right size.
        private DynamicIndexBuffer GetIBOForBatch(Batch batch)
        {
            var config = batch.Config;

            var create = false;
            var had = false;
            DynamicIndexBuffer ibo = null;
            if (_batchIBOTable.TryGetValue(config, out ibo))
            {
                had = true;
                if (ibo.IndexCount != batch.Indicies.Count)
                {
                    create = true;
                }
            }
            else // there wasn't even a buffer!
            {
                create = true;
            }

            if (create)
            {
                var nextIbo = new DynamicIndexBuffer(_device, typeof(short), batch.Indicies.Count, BufferUsage.None);

                // if we used to have a vbo, we need to toss the old one
                if (had)
                {
                    ibo.Dispose();
                    _batchIBOTable[config] = nextIbo;
                }
                else
                {
                    _batchIBOTable.Add(config, nextIbo);
                }

                ibo = nextIbo;
            }

            return ibo;
        }

        private List<VertexPositionColorNormalTexture> ApplyCubeDetails(List<VertexPositionColorNormalTexture> cubeVerts, Vector3 position, Vector3 size, Rotation rotation, Color color,
            TextureStyle style, Vector2 scale, Texture2D texture, bool inAtlas)
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

            var rotationMatrix = Matrix.CreateFromAxisAngle(rotation.Axis, rotation.Radians);


            var output = new List<VertexPositionColorNormalTexture>();

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

                    var vert = cubeVerts[i + start];
                    output.Add(new VertexPositionColorNormalTexture(
                        Vector3.Transform(vert.Position * size, rotationMatrix) + position,
                        color,
                        transformUV(new Vector2(x, y)),
                         Vector3.Transform(vert.Normal, rotationMatrix)));
                }
            }

            return output;
        }

        private VerticiesAndIndicies GetBaseCube(Vector3 position, Vector3 size, Rotation rotation, Color color)
        {
            var cube = UnitCube;
            var axis = rotation.Axis;
            //waxis.Normalize();



            var verts = cube.Verticies
                //.Scale(size)
                //.Rotate(axis, rotation.Radians)
                //.Translate(position);
                .ScaleRotateTranslateColor(position, size, axis, rotation.Radians, color);



            return new VerticiesAndIndicies(verts, cube.Indices);
        }

        private static readonly VerticiesAndIndicies UnitQuad =
            new VerticiesAndIndicies(new VertexPositionColorNormalTexture[]
            {
                new VertexPositionColorNormalTexture(new Vector3(0, 0, 0), Color.White, new Vector2(0, 0), new Vector3(0,0, -1)), 
                new VertexPositionColorNormalTexture(new Vector3(1, 0, 0), Color.White, new Vector2(0, 0), new Vector3(0,0, -1)), 
                new VertexPositionColorNormalTexture(new Vector3(1, 1, 0), Color.White, new Vector2(0, 0), new Vector3(0,0, -1)), 
                new VertexPositionColorNormalTexture(new Vector3(0, 1, 0), Color.White, new Vector2(0, 0), new Vector3(0,0, -1)), 
            }.ToList().Translate(new Vector3(-.5f, -.5f, 0)),
            new short[]
            {
                0, 1, 2, 0, 2, 3
            }.ToList());


        private static readonly VerticiesAndIndicies UnitCube =
            new VerticiesAndIndicies(new VertexPositionColorNormalTexture[]
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


    }
}
