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
using DCG.Framework.PrimtiveBatch;

namespace DCG.Framework.PrimtiveBatch
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

        private RenderTarget2D _colorRT, _normalRT, _depthRT, _lightRT, _finalRT; // the mysterious deferred render targets... 
        private Vector2 _halfPixel;

        private List<DirectionalLight> _directionalLights;
        private List<PointLight> _pointLights;

        private VertexBuffer _pointLightVbuffer;
        private IndexBuffer _pointLightIBuffer;


        // the _batchVBOTable and _batchIBOTable are tables that point that hold an ongoing buffer for a given batch config, across begin/flush cycles.
        private Dictionary<BatchConfig, DynamicVertexBuffer> _batchVBOTable;
        private Dictionary<BatchConfig, DynamicIndexBuffer> _batchIBOTable;

        public Effect ClearGBufferEffect { get; set; }
        public Effect RenderGBufferEffect { get; set; }
        public Effect PassThroughEffect { get; set; }
        public Effect DirectionalLightEffect { get; set; }
        public Effect CombineFinalEffect { get; set; }
        public Effect PointLightEffect { get; set; }
        private FullScreenQuad quad;

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

            _directionalLights = new List<DirectionalLight>();
            _pointLights = new List<PointLight>();


            _pointLightVbuffer = new VertexBuffer(_device, typeof(VertexPositionColorNormalTexture), SphereVerts.Length, BufferUsage.None);
            _pointLightIBuffer = new IndexBuffer(_device, typeof (short), SphereIndicies.Length, BufferUsage.None);
            _pointLightVbuffer.SetData(SphereVerts);
            _pointLightIBuffer.SetData(SphereIndicies);


            HasBegun = false;


            // initialize the deferred RTs
            var bbWidth = device.PresentationParameters.BackBufferWidth;
            var bbHeight = device.PresentationParameters.BackBufferHeight;
            _colorRT = new RenderTarget2D(device, bbWidth, bbHeight, false, SurfaceFormat.Color, DepthFormat.Depth24Stencil8); // TODO watch out for depth format, its shifty... 
            _normalRT = new RenderTarget2D(device, bbWidth, bbHeight, false, SurfaceFormat.Color, DepthFormat.None); // TODO watch out for depth format, its shifty... 
            _depthRT = new RenderTarget2D(device, bbWidth, bbHeight, false, SurfaceFormat.Single, DepthFormat.None); // TODO watch out for depth format, its shifty... 
            _lightRT = new RenderTarget2D(device, bbWidth, bbHeight, false, SurfaceFormat.Color, DepthFormat.Depth24Stencil8); // TODO watch out for depth format, its shifty... 
            _finalRT = new RenderTarget2D(device, bbWidth, bbHeight, false, SurfaceFormat.Color, DepthFormat.Depth24Stencil8); // TODO watch out for depth format, its shifty... 
            _halfPixel = new Vector2(.5f / bbWidth, .5f / bbHeight);

            quad = new FullScreenQuad(_device);

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
            _directionalLights.Clear();
            _pointLights.Clear();
            HasBegun = true;
        }

        /// <summary>
        /// Actually draws all of the cubes that have been called. 
        /// </summary>
        /// <param name="viewMatrix">The view matrix that will be used to draw all the cubes</param>
        /// <param name="projectionMatrix">the projection matrix that will be used to draw all the cubes</param>
        public void Flush(Color ambient, Vector3 camPosition, Matrix viewMatrix, Matrix projectionMatrix)
        {
            if (!HasBegun) // double check that we aren't flushing without ever having started the batch
            {
                throw new Exception("The batch has not been started, and cannot be drawn");
            }


            SetGBuffer();
            ClearGBuffer();
          
            ////// set basic effect parameters. TODO make the effect customizable from outside the PrimitiveBatch
            //////_effect.Projection = projectionMatrix;
            //////_effect.View = viewMatrix;
            //////_effect.World = _worldMatrix;


            _device.DepthStencilState = DepthStencilState.Default;
            _device.RasterizerState = RasterizerState.CullClockwise;
            _device.BlendState = BlendState.Opaque;

            RenderGBufferEffect.Parameters["WorldViewProj"].SetValue(
                _worldMatrix * viewMatrix * projectionMatrix);


            RenderGBufferEffect.Parameters["specularPower"].SetValue(.5f);
            RenderGBufferEffect.Parameters["specularIntensity"].SetValue(1f);

            // time to iterate over all the batches. 
            // each batch will effect graphics device configurations. 
            _batchColl.GetAll().ForEach(batch =>
            {
                // set the texture and sampler state for this batch.
                _effect.Texture = batch.Config.Texture;
                _device.SamplerStates[0] = batch.Config.SamplerState;

                RenderGBufferEffect.Parameters["Texture"].SetValue(batch.Config.Texture);

                RenderGBufferEffect.Parameters.TrySet("NormalMap", batch.Config.NormalMap);

                // get vertex data and index data, and set the graphics device to use them
                var vBuffer = GetVBOForBatch(batch);
                var iBuffer = GetIBOForBatch(batch);

                var verts = batch.VertexArray;

                vBuffer.SetData(batch.VertexArray);
                iBuffer.SetData(batch.IndexArray);
                _device.SetVertexBuffer(vBuffer);
                _device.Indices = iBuffer;

                // actually do the draw call. 

                foreach (EffectPass pass in RenderGBufferEffect.CurrentTechnique.Passes)
                {
                    pass.Apply(); // sends pass to gfx. 
                    _device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, batch.GetIndexArrayLength() / 3);
                }
            });

          
            HasBegun = false;

            

            DrawLights(camPosition, viewMatrix, projectionMatrix);


            _device.RasterizerState = RasterizerState.CullCounterClockwise;
            CombineLights(ambient);


            _device.SetRenderTarget(null);
            int halfWidth = _device.Viewport.Width / 2;
            int halfHeight = _device.Viewport.Height / 2;
           // _device.Clear(Color.Purple);
           // _device.BlendState = BlendState.AlphaBlend;
            //_sb.Begin(SpriteSortMode.Immediate, BlendState.Opaque, SamplerState.LinearWrap, DepthStencilState.Default, RasterizerState.CullNone);
            _sb.Begin();
            //_sb.Draw(_colorRT, Vector2.Zero, Color.White);
            _sb.Draw(_colorRT, new Rectangle(0, 0, halfWidth, halfHeight), Color.White);
            _sb.Draw(_normalRT, new Rectangle(halfWidth, 0, halfWidth, halfHeight), Color.White);
            _sb.Draw(_lightRT, new Rectangle(0, halfHeight, halfWidth, halfHeight), Color.White);
            _sb.Draw(_finalRT, new Rectangle(halfWidth, halfHeight, halfWidth, halfHeight), Color.White);
            _sb.End();

           

            // optionally display the texture atlas. 
            if (AtlasShown)
            {
                //_sb.Begin();
                //_sb.Draw(_atlas.Texture, new Rectangle(0, 0, 100, 100), new Color(1f, 1f, 1f, .5f));
                //_sb.End();
            }


        }

        private void DrawLights(Vector3 camPosition, Matrix view, Matrix proj)
        {

            _device.SetRenderTarget(_lightRT);
            _device.Clear(Color.TransparentBlack);

            var lightBlender = new BlendState();
            lightBlender.AlphaBlendFunction = BlendFunction.Add;
            lightBlender.AlphaSourceBlend = Blend.One;
            lightBlender.AlphaDestinationBlend = Blend.One;
            lightBlender.ColorBlendFunction = BlendFunction.Add;
            lightBlender.ColorSourceBlend = Blend.One;
            lightBlender.ColorDestinationBlend = Blend.One;
            _device.BlendState = lightBlender;


            //draw some lights
            _device.DepthStencilState = DepthStencilState.DepthRead;
            _pointLights.ForEach(d => DrawPointLight(d, camPosition, view, proj));

            _directionalLights.ForEach(d => DrawDirectionalLight(d, camPosition, view, proj));

        }

        private void CombineLights(Color ambient)
        {
            _device.SetRenderTarget(_finalRT);
            CombineFinalEffect.Parameters.TrySet("colorMap", _colorRT);
            //CombineFinalEffect.Parameters["colorMap"].SetValue(_colorRT);
            CombineFinalEffect.Parameters["lightMap"].SetValue(_lightRT);
            CombineFinalEffect.Parameters["halfPixel"].SetValue(_halfPixel);

            CombineFinalEffect.Parameters["ambient"].SetValue(ambient.ToVector4());

            foreach (EffectPass pass in CombineFinalEffect.CurrentTechnique.Passes)
            {
                pass.Apply(); // sends pass to gfx. 
                quad.Draw(_device);
                //quad.Draw(_device, new Vector2(.5f, -.5f), new Vector2(.5f, .5f));
            }
        }

        #endregion


        #region Lighting Methods

        public void LightDirectional(Vector3 direction, Color color)
        {
            // create the light
            var light = new DirectionalLight(direction, color);

            // and stash the light
            _directionalLights.Add(light);
        }

        public void LightPoint(Vector3 position, Color color, float radius, float intensity)
        {
            var light = new PointLight(position, radius, intensity, color);
            _pointLights.Add(light);
        }

        #endregion

        #region Cube Methods
        public void Cube(Vector3 position, Vector3 size, Quaternion rotation)
        {
            Cube(position, size, rotation, Color.White);   
        }

        public void Cube(Vector3 position, Vector3 size, Quaternion rotation, Color color)
        {
            Cube(position, size, rotation, color, null, Vector2.One, Vector2.Zero, SamplerState.PointClamp, TextureStyle.PerQuad);
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

        public void Cube(Vector3 position, Vector3 size, Quaternion rotation, Color color, Texture2D texture,
            Vector2 textureScale, Vector2 textureOffset, SamplerState samplerState, TextureStyle textureStyle)
        {
            Cube(position, size, rotation, color, texture, null, textureScale, textureOffset, samplerState, textureStyle);
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
        public void Cube(Vector3 position, Vector3 size, Quaternion rotation, Color color, Texture2D texture, Texture2D normalMap,
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

            if (normalMap == null)
            {
                normalMap = _pixel;
            }

            if (samplerState == null) // normally we use the texture atlas, and the samplerState is clamped
            {
                samplerState = SamplerState.AnisotropicClamp;
            }
            
            // working variables. 
            Batch batch;
            var inAtlas = false;

            ///samplerState = SamplerState.LinearWrap; // TODO HACK.
            
            if (samplerState.Equals(SamplerState.LinearClamp)
                || samplerState.Equals(SamplerState.PointClamp)
                || samplerState.Equals(SamplerState.AnisotropicClamp)) // any of the xClamp sampler states trigger the usage of the texture atlas
            {
                batch = GetBatch(_atlas.Texture, normalMap, SamplerState.LinearClamp);
                inAtlas = true;
            }
            else // any other sampler state has its own batch.
            {
                batch = GetBatch(texture,normalMap, SamplerState.LinearWrap);
            }
            var oldVertexCount = batch.GetVertexArrayLength();

            // this is jenky. 
            // the ApplyCubeDetails function is going to put vertex data directly into the batch
            ApplyCubeDetails(batch, position, size, rotation, color, textureStyle, textureScale, textureOffset, texture, inAtlas);
            // and the addcubeIndicies function will add index data directly into the batch
            //batch.AddCubeIndicies();

            batch.AddSomeIndicies(CubeIndicies, (short)oldVertexCount);
        }

        #endregion

        #region Sphere Methods

        public void Sphere(Vector3 position, Vector3 size, Quaternion rotation, Color color, Texture2D texture)
        {

            // TODO implement rotation

            if (texture == null)
            {
                texture = _pixel;
            }

            var batch = GetBatch(texture, _pixel,SamplerState.LinearWrap);

            var oldVertexCount = batch.GetVertexArrayLength();

            var textureOffset = Vector2.Zero;
            var textureScale = new Vector2(1, -1);
            var inAtlas = false;

            var applyTexOffset = new Func<Vector2, Vector2>(v => (v + textureOffset));

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

            var transformMatrix = Matrix.CreateFromQuaternion(rotation);
            for (var i = 0; i < SphereVerts.Length; i++)
            {
                var vert = SphereVerts[i];


                batch.AddVertex(new VertexPositionColorNormalTexture(
                    Vector3.Transform(position + vert.Position * size, transformMatrix),
                    color,
                    transformUV(vert.TextureCoordinate),
                    Vector3.Transform(vert.Normal, transformMatrix)));

            }
            batch.AddSomeIndicies(SphereIndicies, (short) oldVertexCount);
        }

        #endregion


        #region Helper Methods

        private Batch GetBatch(Texture2D texture, Texture2D normalMap, SamplerState state)
        {
            var conf = new BatchConfig(texture, normalMap, PrimitiveType.TriangleList, state, BlendState.NonPremultiplied);
            var b = _batchColl.Get(conf);
            return b;
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


        private void DrawPointLight(PointLight light, Vector3 camPosition, Matrix view, Matrix projection)
        {
            PointLightEffect.Parameters["colorMap"].SetValue(_colorRT);
            PointLightEffect.Parameters["normalMap"].SetValue(_normalRT);
            PointLightEffect.Parameters["depthMap"].SetValue(_depthRT);
            //compute the light world matrix
            //scale according to light radius, and translate it to light position
            Matrix sphereWorldMatrix = Matrix.CreateScale(light.Radius) * Matrix.CreateTranslation(light.Position);

            PointLightEffect.Parameters["World"].SetValue(sphereWorldMatrix);
            PointLightEffect.Parameters["View"].SetValue(view);
            PointLightEffect.Parameters["Projection"].SetValue(projection);
            //light position
            PointLightEffect.Parameters["lightPosition"].SetValue(light.Position);
            //set the color, radius and Intensity
            PointLightEffect.Parameters["Color"].SetValue(light.Color.ToVector3());
            PointLightEffect.Parameters["lightRadius"].SetValue(light.Radius);
            PointLightEffect.Parameters["lightIntensity"].SetValue(light.Intensity);
            //parameters for specular computations
            PointLightEffect.Parameters["cameraPosition"].SetValue(camPosition);
            PointLightEffect.Parameters["InvertViewProjection"].SetValue(Matrix.Invert(view * projection));
            //size of a halfpixel, for texture coordinates alignment
            PointLightEffect.Parameters["halfPixel"].SetValue(_halfPixel);

            float cameraToCenter = Vector3.Distance(camPosition, light.Position);
            //if we are inside the light volume, draw the sphere's inside face
            if (cameraToCenter < light.Radius)
            {

                _device.RasterizerState = RasterizerState.CullCounterClockwise;
            }
            else
            {
                _device.RasterizerState = RasterizerState.CullClockwise;
            }
            //_device.RasterizerState = RasterizerState.CullNone;

            var dss = new DepthStencilState();

           

            dss.DepthBufferWriteEnable = true;
            dss.DepthBufferEnable = true;
            dss.DepthBufferFunction = CompareFunction.Greater;

            //_device.DepthStencilState = dss;

            _device.SetVertexBuffer(_pointLightVbuffer);
            _device.Indices = _pointLightIBuffer;
            foreach (EffectPass pass in PointLightEffect.CurrentTechnique.Passes)
            {
                pass.Apply(); // sends pass to gfx. 
                _device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, SphereIndicies.Length / 3);
            }


        }

        private void DrawDirectionalLight(DirectionalLight light, Vector3 camPosition, Matrix view, Matrix projection)
        {

            //set all parameters
            DirectionalLightEffect.Parameters["colorMap"].SetValue(_colorRT);

            DirectionalLightEffect.Parameters["normalMap"].SetValue(_normalRT);
            DirectionalLightEffect.Parameters["depthMap"].SetValue(_depthRT);
            DirectionalLightEffect.Parameters["lightDirection"].SetValue(light.Direction);
            DirectionalLightEffect.Parameters["Color"].SetValue(light.Color.ToVector3());
            DirectionalLightEffect.Parameters["cameraPosition"].SetValue(camPosition);
            DirectionalLightEffect.Parameters["InvertViewProjection"].SetValue(Matrix.Invert(view * projection));
            DirectionalLightEffect.Parameters["halfPixel"].SetValue(_halfPixel);

            _device.RasterizerState = RasterizerState.CullCounterClockwise;
            //_device.DepthStencilState = DepthStencilState.None;
           
            foreach (EffectPass pass in DirectionalLightEffect.CurrentTechnique.Passes)
            {
                pass.Apply(); // sends pass to gfx. 
                quad.Draw(_device);
            }

        }

        

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

            _device.BlendState = BlendState.Opaque;
            _device.RasterizerState = RasterizerState.CullNone;
            foreach (EffectPass pass in ClearGBufferEffect.CurrentTechnique.Passes)
            {
                pass.Apply(); // sends pass to gfx. 
                quad.Draw(_device);
            }

        }


        #endregion


        #region Unit Elements


        static PrimitiveBatch()
        {
            // work in here to create sphere points. 
            SphereVerts = MakeUnitSphere(20, 20);


        }


        private static readonly short[] CubeIndicies = new short[]
        {
            0, 1, 2, 0, 2, 3,
            4, 5, 6, 4, 6, 7,
            8, 9, 10, 8, 10, 11,
            12, 13, 14, 12, 14, 15,
            16, 17, 18, 16, 18, 19,
            20, 21, 22, 20, 22, 23
        };
        private static short[] SphereIndicies;
        private static VertexPositionColorNormalTexture[] MakeUnitSphere(int circleNum, int radialNum)
        {

            var verts = new List<VertexPositionColorNormalTexture>();

            // circleNum * radiulNum * 6 + 6 * radialNum 
            // 6 * radialNum (circleNum + 1)


            for (var i = 0f; i < circleNum; i++)
            {
                var v = (i + 1)/(circleNum + 1);

                for (var j = 0; j <= radialNum; j++)
                {
                    var theta = j*MathHelper.TwoPi/radialNum; // todo optimize speed

                    // new style
                    var y = (float)Math.Sin(MathHelper.Pi * v - MathHelper.PiOver2);
                    var modifiedRadius = Math.Sqrt(1 - y * y);
                    
                    var x = (float) ( modifiedRadius*Math.Cos(theta) );
                    var z = (float) ( modifiedRadius*Math.Sin(theta) );
                   
                    var normal = new Vector3(x, y, z);

                    //if (Math.Abs(normal.Length() - 1f) > 0.001f || float.IsNaN(x) || float.IsNaN(y) || float.IsNaN(z))
                    //{
                    //    Console.WriteLine("NOOOOO");
                    //}

                    normal.Normalize(); // todo maybe we don't need this depending on WHEN the radius multiplication happens

                    var uv_v = v;//(y + 1) * .5f;
                    var uv_u = j / (float)radialNum;
                    //Console.WriteLine("UV COORD "  + uv_v);
                    verts.Add(new VertexPositionColorNormalTexture(
                        new Vector3(x, y, z), Color.White, new Vector2(1 - uv_u, uv_v), normal));


                  
                }
            }

            var top = new Vector3(0, 1, 0);
            var bot = new Vector3(0, -1, 0);

            // THE TOP SET OF POINTS
            var topIndexStart = verts.Count;
            for (var i = 0; i <= radialNum; i++)
            {
                var v = 1;
                var u = i / (float)radialNum;
                verts.Add(new VertexPositionColorNormalTexture(
                    top, Color.White, new Vector2(1 - u, v), Vector3.UnitY));
            }

            //// THE BOT SET OF POINTS
            var botIndexStart = verts.Count;
            for (var i = 0; i <= radialNum; i++)
            {
                var v = 0;
                var u = i / (float)radialNum;
                verts.Add(new VertexPositionColorNormalTexture(
                    bot, Color.White, new Vector2(1- u, v), -Vector3.UnitY));
            }

            //verts.Add(new VertexPositionColorNormalTexture(
            //    top, Color.LimeGreen, new Vector2(.5f, 1), Vector3.UnitY));
            //var topIndex = verts.Count - 1;

            //verts.Add(new VertexPositionColorNormalTexture(
            //    bot, Color.LimeGreen, new Vector2(.5f, 0), -Vector3.UnitY));
            //var botIndex = verts.Count - 1;


            // making the indecies!

            var indicies = new List<short>();
            for (var i = 0; i < circleNum - 1; i++)
            {
                for (var j = 0; j < radialNum; j++)
                {
                   
                    

                    var topLeft = (short)(i * (radialNum + 1) + j);
                    var topRight = (short) ((topLeft + 1) );
                    //if (topRight >= (i + 1) * radialNum)
                    //{
                    //    topRight -= (short)radialNum;
                    //}


                    var botLeft = (short) (topLeft + radialNum + 1);
                    var botRight = (short) ( (botLeft + 1)  );
                    //if (botRight >= (i + 2) * radialNum)
                    //{
                    //    botRight -= (short)radialNum;
                    //}


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

                indicies.Add((short)(botIndexStart + j));
                indicies.Add(j);
                indicies.Add((short)((j + 1)));


                indicies.Add((short)(topIndexStart + j));
                indicies.Add((short)((circleNum - 1) * (radialNum + 1) + j + 1));
                indicies.Add((short)((circleNum - 1) * (radialNum + 1) + j));



                //indicies.Add((short)(calcVertNum(circleNum - 1, 0) + ((j + 1) % radialNum)));
                //indicies.Add((short)(calcVertNum(circleNum - 1, 0) + j));

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
