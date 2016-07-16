using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Globalization;
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

        private RenderTarget2D _colorRT, _normalRT, _depthRT; // the mysterious deferred render targets... 

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
            //_effect.Alpha = 1.0f;
            //_effect.VertexColorEnabled = true;
            //_effect.TextureEnabled = true;
            //_effect.PreferPerPixelLighting = true;
           // _effect.LightingEnabled = true;
         //   _effect.EnableDefaultLighting();

            HasBegun = false;


            // initialize the deferred RTs
            var bbWidth = device.PresentationParameters.BackBufferWidth;
            var bbHeight = device.PresentationParameters.BackBufferHeight;
            _colorRT = new RenderTarget2D(device, bbWidth, bbHeight, false, 
                SurfaceFormat.Color, DepthFormat.Depth24); // TODO watch out for depth format, its shifty... 
            _normalRT = new RenderTarget2D(device, bbWidth, bbHeight, false, SurfaceFormat.Color, DepthFormat.Depth24); // TODO watch out for depth format, its shifty... 
            _depthRT = new RenderTarget2D(device, bbWidth, bbHeight, false, SurfaceFormat.Single, DepthFormat.Depth24); // TODO watch out for depth format, its shifty... 




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


            SetGBuffer();
            ClearGBuffer();
            //scene.DrawScene(camera, gameTime);
           

           
            // set basic effect parameters. TODO make the effect customizable from outside the PrimitiveBatch
            _effect.Projection = projectionMatrix;
            _effect.View = viewMatrix;
            _effect.World = _worldMatrix;

           
            _device.DepthStencilState = DepthStencilState.Default;
            _device.RasterizerState = RasterizerState.CullClockwise;
            _device.BlendState = BlendState.Opaque;
            //RenderGBufferEffect.Parameters["World"].SetValue(Matrix.Identity);
            //RenderGBufferEffect.Parameters["View"].SetValue(viewMatrix);
            //RenderGBufferEffect.Parameters["Projection"].SetValue(projectionMatrix);

            RenderGBufferEffect.Parameters["WorldViewProj"].SetValue(
                _worldMatrix * viewMatrix * projectionMatrix);

            // time to iterate over all the batches. 
            // each batch will effect graphics device configurations. 
            _batchColl.GetAll().ForEach(batch =>
            {
                // set the texture and sampler state for this batch.
                _effect.Texture = batch.Config.Texture;
                _device.SamplerStates[0] = batch.Config.SamplerState;
                
                RenderGBufferEffect.Parameters["Texture"].SetValue(batch.Config.Texture);

                // get vertex data and index data, and set the graphics device to use them
                var vBuffer = GetVBOForBatch(batch);
                var iBuffer = GetIBOForBatch(batch);

                var verts = batch.VertexArray;

                vBuffer.SetData(batch.VertexArray);
                iBuffer.SetData(batch.IndexArray);
                _device.SetVertexBuffer(vBuffer);
                _device.Indices = iBuffer;

                // actually do the draw call. 

                //RenderGBufferEffect.CurrentTechnique = RenderGBufferEffect.Techniques["Technique1"];
                foreach (EffectPass pass in RenderGBufferEffect.CurrentTechnique.Passes)
                {
                    pass.Apply(); // sends pass to gfx. 
                    _device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, batch.GetIndexArrayLength() / 3);
                }
            });

          
            HasBegun = false;

            ResolveGBuffer();


            int halfWidth = _device.Viewport.Width / 2;
            int halfHeight = _device.Viewport.Height / 2;
            _device.Clear(Color.White);
            _sb.Begin(SpriteSortMode.Immediate,
                BlendState.Opaque);
            //_sb.Draw(_normalRT, Vector2.Zero, Color.White);
            _sb.Draw(_colorRT, new Rectangle(0, 0, halfWidth, halfHeight), Color.White);
            _sb.Draw(_normalRT, new Rectangle(halfWidth, 0, halfWidth, halfHeight), Color.White);
            _sb.Draw(_depthRT, new Rectangle(0, halfHeight, halfWidth, halfHeight), Color.White);
            _sb.End();


            // optionally display the texture atlas. 
            if (AtlasShown)
            {
                _sb.Begin();
                _sb.Draw(_atlas.Texture, new Rectangle(0, 0, 100, 100), new Color(1f, 1f, 1f, .5f));
                _sb.End();
            }


        }

        #endregion

        #region Cube Methods
        public void Cube(Vector3 position, Vector3 size, Quaternion rotation)
        {
            Cube(position, size, rotation, Color.White);   
        }

        public void Cube(Vector3 position, Vector3 size, Quaternion rotation, Color color)
        {
            Cube(position, size, rotation, color, null, Vector2.One, Vector2.Zero, SamplerState.LinearClamp, TextureStyle.PerQuad);
        }

        public void Cube(Vector3 position, Vector3 size, Quaternion rotation, Texture2D texture,
            TextureStyle textureStyle = TextureStyle.PerQuad)
        {
            Cube(position, size, rotation, Color.White, texture, Vector2.One, Vector2.Zero, SamplerState.LinearClamp, textureStyle);
        }

        public void Cube(Vector3 position, Vector3 size, Quaternion rotation, Texture2D texture, Vector2 textureScale,
            SamplerState samplerState, TextureStyle textureStyle = TextureStyle.PerQuad)
        {
            Cube(position, size, rotation, Color.White, texture, textureScale, Vector2.Zero, samplerState, textureStyle);
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
        public void Cube(Vector3 position, Vector3 size, Quaternion rotation, Color color, Texture2D texture,
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

        #region Sphere Methods

        public void Sphere(Vector3 position, Vector3 size, float radius)
        {
            // do it all.

            var batch = GetBatch(_pixel, SamplerState.LinearWrap);

            var oldVertexCount = batch.GetVertexArrayLength();

            for (var i = 0; i < SphereVerts.Length; i++)
            {
                var vert = SphereVerts[i];
                batch.AddVertex(new VertexPositionColorNormalTexture(
                    position + vert.Position * size * radius, vert.Color, vert.TextureCoordinate, vert.Normal));

            }
            batch.AddSomeIndicies(SphereIndicies, (short) oldVertexCount);
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
        private void ApplyCubeDetails(Batch batch, Vector3 position, Vector3 size, Quaternion rotation, Color color,
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

            var rotationMatrix = Matrix.CreateFromQuaternion(rotation);
            //var rotationMatrix = Matrix.CreateFromAxisAngle(rotation.Axis, rotation.Radians);
            for (int s = 0; s < 6; s++)
            {
                var start = s * 4;
                for (int i = 0; i < 4; i++)
                {
                    var x = (i == 1 || i == 2 ? s : (s + 1)) / 6f;
                    var y = i == 2 || i == 3 ? 0 : 1;

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

        #region DeferredFunctions

        public Effect ClearGBufferEffect { get; set; }
        public Effect RenderGBufferEffect { get; set; }
        public Effect PassThroughEffect { get; set; }

        private void SetGBuffer()
        {
          
           _device.SetRenderTargets(_colorRT, _normalRT, _depthRT);
        }

        private void ResolveGBuffer()
        {
            _device.SetRenderTarget(null);
        }

        private void ClearGBuffer()
        {

            _sb.Begin(SpriteSortMode.Immediate,
                BlendState.NonPremultiplied,
                SamplerState.PointClamp,
                DepthStencilState.Default,
                RasterizerState.CullNone,
                ClearGBufferEffect);
            _sb.Draw(_pixel,
                new Rectangle(-1, -1, 2, 2), Color.White);

            _sb.End();
        }


        #endregion


        #region Unit Elements


        static PrimitiveBatch()
        {
            // work in here to create sphere points. 
            SphereVerts = MakeUnitSphere(12, 15);


        }

        private static short[] SphereIndicies;
        private static VertexPositionColorNormalTexture[] MakeUnitSphere(int circleNum, int radialNum)
        {

            var verts = new List<VertexPositionColorNormalTexture>();

            var indicies = new List<short>();
            // circleNum * radiulNum * 6 + 6 * radialNum 
            // 6 * radialNum (circleNum + 1)


            for (var i = 0f; i < circleNum; i++)
            {
                var totalCircleNum = circleNum + 1;
                var v = (i + 1)/totalCircleNum;

                for (var j = 0; j < radialNum; j++)
                {
                    var theta = j*MathHelper.TwoPi/radialNum; // todo optimize speed

                    var y = (v - .5f) * 2;
                    //var modifiedRadius = .5f * radius * Math.Sin(v * Math.PI);
                    var stupid = ( v*2 -1);
                    var stupidArg = 1 - Math.Pow(stupid, 2);

                    var modifiedRadius = Math.Sqrt(stupidArg);

                    
                    // input is [0,1]
                    // output [1, 0, 1]     0 -> 1,     .5 -> 0,    1 -> 1


                    var x = (float) ( modifiedRadius*Math.Cos(theta) );
                    var z = (float) ( modifiedRadius*Math.Sin(theta) );
                   
                    var normal = new Vector3(x, y, z);

                    if (Math.Abs(normal.Length() - 1f) > 0.001f || float.IsNaN(x) || float.IsNaN(y) || float.IsNaN(z))
                    {
                        Console.WriteLine("NOOOOO");
                    }

                    normal.Normalize(); // todo maybe we don't need this depending on WHEN the radius multiplication happens

                    verts.Add(new VertexPositionColorNormalTexture(
                        new Vector3(x, y, z), Color.LimeGreen, Vector2.Zero, normal));


                  
                }
            }

            // TODO later, generate top and bott
            var top = new Vector3(0, 1, 0);
            var bot = new Vector3(0, -1, 0);

            verts.Add(new VertexPositionColorNormalTexture(
                top, Color.LimeGreen, Vector2.Zero, Vector3.UnitY));
            var topIndex = verts.Count - 1;

            verts.Add(new VertexPositionColorNormalTexture(
                bot, Color.LimeGreen, Vector2.Zero, -Vector3.UnitY));
            var botIndex = verts.Count - 1;


            // making the indecies!
            var calcVertNum = new Func<int, int, int>((i, j) => i*radialNum + j);
            for (var i = 0; i < circleNum -1; i++)
            {
                for (var j = 0; j < radialNum; j++)
                {
                    var topLeft = (short)(i * radialNum + j);
                    var topRight = (short) ((topLeft + 1) );
                    if (topRight >= (i + 1)*radialNum)
                    {
                        topRight -= (short) radialNum;
                    }


                    var botLeft = (short) (topLeft + radialNum);
                    var botRight = (short) ( (botLeft + 1)  );
                    if (botRight >= (i+2) *radialNum)
                    {
                        botRight -= (short)radialNum;
                    }

                    if (topLeft == 5 || topRight == 5 || botLeft == 5 || botRight == 5)
                    {
                        
                    }

                    indicies.Add(  topLeft);
                    indicies.Add(  botLeft);
                    indicies.Add(  botRight);

                    indicies.Add(  topLeft);
                    indicies.Add(  botRight);
                    indicies.Add(  topRight);
                }
            }

            for (short j = 0; j < radialNum; j++)
            {
                indicies.Add((short)botIndex);
                indicies.Add(j);
                indicies.Add((short)((j + 1) % radialNum));

                indicies.Add((short)topIndex);
                indicies.Add((short)(calcVertNum(circleNum - 1, 0) + j));
                indicies.Add((short)(calcVertNum(circleNum - 1, 0) + ((j + 1) % radialNum)));

            }

            SphereIndicies = indicies.ToArray();
            return verts.ToArray();
        }


        private static VertexPositionColorNormalTexture[] SphereVerts = null; 



        /// <summary>
        /// This is a unit quad.
        /// </summary>
        private static readonly List<VertexPositionColorNormalTexture> UnitQuad =
            new VertexPositionColorNormalTexture[]
            {
                
                new VertexPositionColorNormalTexture(new Vector3(0, 1, 0), Color.White, new Vector2(0, 0),
                    new Vector3(0, 0, -1)), // 4
                
                
                new VertexPositionColorNormalTexture(new Vector3(1, 1, 0), Color.White, new Vector2(0, 0),
                    new Vector3(0, 0, -1)), // 3

                    new VertexPositionColorNormalTexture(new Vector3(1, 0, 0), Color.White, new Vector2(0, 0),
                    new Vector3(0, 0, -1)), // 2

                    new VertexPositionColorNormalTexture(new Vector3(0, 0, 0), Color.White, new Vector2(0, 0),
                    new Vector3(0, 0, -1)), // 1 
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
