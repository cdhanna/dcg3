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

        #region Private Fields...

        private readonly GraphicsDevice _device;
        private readonly Texture2D _pixel;

        private readonly SpriteBatch _sb;
        private readonly BasicEffect _effect;

        private readonly Matrix _worldMatrix;

        private readonly BatchCollection _batchColl;
        private readonly TextureAtlas _atlas;

        // the _batchVBOTable and _batchIBOTable are tables that point that hold an ongoing buffer for a given batch config, across begin/flush cycles.
        private Dictionary<BatchConfig, DynamicVertexBuffer> _batchVBOTable;
        private Dictionary<BatchConfig, DynamicIndexBuffer> _batchIBOTable;
        
        #endregion

        #region Public Properties ...

        public bool HasBegun { get; private set; }
        public bool AtlasShown { get; set; }

        #endregion

        #region Constructor

        public PrimitiveBatch(GraphicsDevice device)
        {
            // grab graphics device info
            _device = device;
            _sb = new SpriteBatch(device);

            // create a utility pixel texture to use if no texture is supplied
            _pixel = new Texture2D(device, 1, 1, false, SurfaceFormat.Color);
            _pixel.SetData(new Color[] { Color.White });

            // create a texture atlas and a batch collection
            AtlasShown = false;
            _atlas = new TextureAtlas(device, 1024);
            _atlas.EnsureTexture(_pixel);
            _batchColl = new BatchCollection();

            // create buffer tables
            _batchIBOTable = new Dictionary<BatchConfig, DynamicIndexBuffer>();
            _batchVBOTable = new Dictionary<BatchConfig, DynamicVertexBuffer>();

            // configure basic world matrix
            _worldMatrix = Matrix.CreateWorld(Vector3.Zero, Vector3.Forward, Vector3.Up);

            // make a basic effect that accepts our inputs
            _effect = new BasicEffect(device);
            _effect.Alpha = 1.0f;
            _effect.VertexColorEnabled = true;
            _effect.TextureEnabled = true;
            _effect.PreferPerPixelLighting = true;
            _effect.LightingEnabled = true;
            _effect.EnableDefaultLighting();

            HasBegun = false;
        }

        #endregion

        #region Begin and Flush

        /// <summary>
        /// Begining the PrimitiveBatch means that all of the existing batches are cleared out.
        /// The texture atlas doesn't get reset, and the VBO/IBOs may stay alive.
        /// </summary>
        public void Begin()
        {
            if (HasBegun)
            {
                throw new Exception("The batch has already begun. It must be ended before another can begin");
            }
            _batchColl.Clear();
            HasBegun = true;
        }

        /// <summary>
        /// Actually draws all of the cubes that have been called. 
        /// </summary>
        /// <param name="viewMatrix">The view matrix that will be used to draw all the cubes</param>
        /// <param name="projectionMatrix">the projection matrix that will be used to draw all the cubes</param>
        public void Flush(Matrix viewMatrix, Matrix projectionMatrix)
        {
            if (!HasBegun) // double check that we aren't flushing without ever having started the batch
            {
                throw new Exception("The batch has not been started, and cannot be drawn");
            }

            // set basic effect parameters. TODO make the effect customizable from outside the PrimitiveBatch
            _effect.Projection = projectionMatrix;
            _effect.View = viewMatrix;
            _effect.World = _worldMatrix;
            _device.DepthStencilState = DepthStencilState.Default;
            _device.RasterizerState = RasterizerState.CullCounterClockwise;

            // time to iterate over all the batches. 
            // each batch will effect graphics device configurations. 
            _batchColl.GetAll().ForEach(batch =>
            {
                // set the texture and sampler state for this batch.
                _effect.Texture = batch.Config.Texture;
                _device.SamplerStates[0] = batch.Config.SamplerState;

                // get vertex data and index data, and set the graphics device to use them
                var vBuffer = GetVBOForBatch(batch);
                var iBuffer = GetIBOForBatch(batch);
                vBuffer.SetData(batch.VertexArray);
                iBuffer.SetData(batch.IndexArray);
                _device.SetVertexBuffer(vBuffer);
                _device.Indices = iBuffer;

                // actually do the draw call. 
                foreach (EffectPass pass in _effect.CurrentTechnique.Passes)
                {
                    pass.Apply(); // sends pass to gfx. 
                    _device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, batch.GetIndexArrayLength() / 3);
                }

            });

            // optionally display the texture atlas. 
            if (AtlasShown)
            {
                _sb.Begin();
                _sb.Draw(_atlas.Texture, new Rectangle(0, 0, 100, 100), new Color(1f, 1f, 1f, .5f));
                _sb.End();
            }

            HasBegun = false;
        }

        #endregion

        #region Cube Methods
        public void Cube(Vector3 position, Vector3 size, Rotation rotation)
        {
            Cube(position, size, rotation, Color.White);   
        }

        public void Cube(Vector3 position, Vector3 size, Rotation rotation, Color color)
        {
            Cube(position, size, rotation, color, null, Vector2.One, Vector2.One, SamplerState.LinearClamp, TextureStyle.PerQuad);
        }

        public void Cube(Vector3 position, Vector3 size, Rotation rotation, Texture2D texture,
            TextureStyle textureStyle = TextureStyle.PerQuad)
        {
            Cube(position, size, rotation, Color.White, texture, Vector2.One, Vector2.One, SamplerState.LinearClamp, textureStyle);
        }

        public void Cube(Vector3 position, Vector3 size, Rotation rotation, Texture2D texture, Vector2 textureScale,
            SamplerState samplerState, TextureStyle textureStyle = TextureStyle.PerQuad)
        {
            Cube(position, size, rotation, Color.White, texture, textureScale, Vector2.One, samplerState, textureStyle);
        }

        /// <summary>
        /// Adds a cube to be drawn when flush() happens
        /// </summary>
        /// <param name="position">the position of the center of the cube</param>
        /// <param name="size">the size of the cube</param>
        /// <param name="rotation">the rotation of the cube</param>
        /// <param name="color">the color of the cube</param>
        /// <param name="texture">the texture the cube will use</param>
        /// <param name="textureScale">a texture coordinate scaling vector, used to tile textures</param>
        /// <param name="samplerState">the sampler state that the graphics device will be when this cube is actually drawn</param>
        /// <param name="textureStyle">the texture style for applying the texture to the cube</param>
        public void Cube(Vector3 position, Vector3 size, Rotation rotation, Color color, Texture2D texture,
            Vector2 textureScale, Vector2 textureOffset, SamplerState samplerState, TextureStyle textureStyle)
        {

            if (!HasBegun) // double check to make the user isn't drawing stuff without beginning the process
            {
                throw new Exception("The batch has not begun, and cannot be drawn to");
            }

            if (texture == null) // we need a texture, so if the user didn't provide one, then give the standard white pixel
            {
                texture = _pixel;
            }

            if (samplerState == null) // normally we use the texture atlas, and the samplerState is clamped
            {
                samplerState = SamplerState.LinearClamp;
            }

            // working variables. 
            Batch batch;
            var inAtlas = false;

            
            if (samplerState.Equals(SamplerState.LinearClamp)
                || samplerState.Equals(SamplerState.PointClamp)
                || samplerState.Equals(SamplerState.AnisotropicClamp)) // any of the xClamp sampler states trigger the usage of the texture atlas
            {
                batch = GetBatch(_atlas.Texture, SamplerState.LinearClamp);
                inAtlas = true;
            }
            else // any other sampler state has its own batch.
            {
                batch = GetBatch(texture, SamplerState.LinearWrap);
            }

            // this is jenky. 
            // the ApplyCubeDetails function is going to put vertex data directly into the batch
            ApplyCubeDetails(batch, position, size, rotation, color, textureStyle, textureScale, textureOffset, texture, inAtlas);
            // and the addcubeIndicies function will add index data directly into the batch
            batch.AddCubeIndicies();

        }

        #endregion

        #region Helper Methods

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
            if (_batchVBOTable.TryGetValue(config, out vbo)) // try and get the existing vbo
            {
                had = true;
                if (vbo.VertexCount != batch.VertexArray.Length) // crap, the size has changed, we need to recreate it.
                {
                    create = true;
                }
            }
            else // there wasn't even a buffer!
            {
                create = true;
            }

            if (create) // we need to create a new vbo
            {
                var nextVbo = new DynamicVertexBuffer(_device, typeof(VertexPositionColorNormalTexture), batch.VertexArray.Length, BufferUsage.None);
                
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
                if (ibo.IndexCount != batch.IndexArray.Length)
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
                var nextIbo = new DynamicIndexBuffer(_device, typeof(short), batch.IndexArray.Length, BufferUsage.None);

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

        /// <summary>
        /// This function will generate the vertex data per cube.
        /// All the translation, rotation, scaling, coloring, and texturing has been baked into this one function, which makes it speedy.
        /// Data is inserted DIRECTLY into the Batch
        /// </summary>
        /// <param name="batch">The batch that this cube will be belong to</param>
        /// <param name="position">The center of the cube</param>
        /// <param name="size">the textureScale of the cube</param>
        /// <param name="rotation">the rotation of the cube</param>
        /// <param name="color">the color of the cube</param>
        /// <param name="style">the texture style of the cube</param>
        /// <param name="textureScale">the texture coordinate textureScale</param>
        /// <param name="texture">the actual texture (not ever an atlas)</param>
        /// <param name="inAtlas">should we be putting the texture inside the atlas</param>
        private void ApplyCubeDetails(Batch batch, Vector3 position, Vector3 size, Rotation rotation, Color color,
            TextureStyle style, Vector2 textureScale, Vector2 textureOffset, Texture2D texture, bool inAtlas)
        {

            var applyTexOffset = new Func<Vector2, Vector2>( v => (v + textureOffset));

            // the transformUV func is used to modify the standard cube UV coordinate to map into atlas version, or to simply wrap the texture around the cube. 
            var transformUV = new Func<Vector2, Vector2>(v => applyTexOffset(v) * textureScale); // this is the 'wrap' version, where the atlas isn't being used.

            var texIndex = new TextureAtlasIndex(); // start as undefined.
            if (inAtlas)
            {
                texIndex = _atlas.EnsureTexture(texture); // plot the texture in the atlas

                // set the func to transform the UV to the atlas coordinate system.
                var atlasSize = new Vector2(_atlas.TextureSize, _atlas.TextureSize);
                var ratio = texIndex.Size / atlasSize;
                transformUV = (v => (applyTexOffset(v) * textureScale) * ratio + texIndex.Position / atlasSize); // this is the 'atlas' version
            }

            var rotationMatrix = Matrix.CreateFromAxisAngle(rotation.Axis, rotation.Radians);
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

                    var vert = UnitCube[i + start];
                    var outputVert = new VertexPositionColorNormalTexture(
                        Vector3.Transform(vert.Position * size, rotationMatrix) + position,
                        color,
                        transformUV(new Vector2(x, y)),
                         Vector3.Transform(vert.Normal, rotationMatrix));

                    batch.AddVertex(outputVert);
                }
            }
           

        }

        #endregion

        #region Unit Elements

        /// <summary>
        /// This is a unit quad.
        /// </summary>
        private static readonly List<VertexPositionColorNormalTexture> UnitQuad =
            new VertexPositionColorNormalTexture[]
            {
                new VertexPositionColorNormalTexture(new Vector3(0, 0, 0), Color.White, new Vector2(0, 0),
                    new Vector3(0, 0, -1)),
                new VertexPositionColorNormalTexture(new Vector3(1, 0, 0), Color.White, new Vector2(0, 0),
                    new Vector3(0, 0, -1)),
                new VertexPositionColorNormalTexture(new Vector3(1, 1, 0), Color.White, new Vector2(0, 0),
                    new Vector3(0, 0, -1)),
                new VertexPositionColorNormalTexture(new Vector3(0, 1, 0), Color.White, new Vector2(0, 0),
                    new Vector3(0, 0, -1)),
            }.ToList().Translate(new Vector3(-.5f, -.5f, 0));

        /// <summary>
        /// This is a unit cube. 
        /// </summary>
        private static readonly List<VertexPositionColorNormalTexture> UnitCube =
            new VertexPositionColorNormalTexture[]
            {

            }.ToList()
                .Concat(UnitQuad
                    .Rotate(Vector3.UnitY, MathHelper.PiOver2)
                    .Rotate(Vector3.UnitX, MathHelper.Pi)
                    .Translate(Vector3.UnitX*-.5f)) // left

                .Concat(UnitQuad
                    .Rotate(Vector3.UnitX, MathHelper.Pi)
                    .Translate(Vector3.UnitZ*.5f)) // back

                .Concat(UnitQuad
                    .Rotate(Vector3.UnitY, -MathHelper.PiOver2)
                    .Rotate(Vector3.UnitX, MathHelper.Pi)
                    .Translate(Vector3.UnitX*.5f)) // right

                .Concat(UnitQuad
                    .Rotate(Vector3.UnitZ, MathHelper.Pi)
                    .Translate(Vector3.UnitZ*-.5f)) // front

                .Concat(UnitQuad
                    .Rotate(Vector3.UnitX, -MathHelper.PiOver2)
                    .Translate(Vector3.UnitY*-.5f)) // bot
                .Concat(UnitQuad
                    .Rotate(Vector3.UnitX, MathHelper.PiOver2)
                    .Translate(Vector3.UnitY*.5f)) // top
            ;

        #endregion

    }
}
