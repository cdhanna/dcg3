using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using DCG.Framework.PrimtiveBatch.ObjLoader;
using DCG3.GameLogic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using DCG.Framework;
using DCG.Framework.Menu;
using DCG.Framework.PrimtiveBatch;
using DCG.Framework.Util;

namespace DCG3
{

    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class TestGame : Game
    {
        private GraphicsDeviceManager _graphics;
        private PrimitiveBatch _pBatch;

        
        private SimpleCamera _cam;

        private Texture2D _texGlobe, _texAgu, _texNuke, _texC, _texAguStrip, _texBorderGlow, _cellTex, _stripeTex, _texFloor;
        private Texture2D _pillowColor, _pillowNormal;
        private Texture2D _globeColor, _globeNormal;
        
        private Rand _rand;
        private FPSHelper _fps;

        private Level _level;
        private Player _plr;

        private DcgModel _model;

        public TestGame()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {
            _pBatch = new PrimitiveBatch(GraphicsDevice);
            _cam = new SimpleCamera(GraphicsDevice);

            _texAgu = Content.Load<Texture2D>("agu");
            _texAguStrip = Content.Load<Texture2D>("agustrip");
            _texNuke = Content.Load<Texture2D>("nuke");
            _texC = Content.Load<Texture2D>("C");
            _texBorderGlow = Content.Load<Texture2D>("glowborder");
            _cellTex = Content.Load<Texture2D>("cell");
            _stripeTex = Content.Load<Texture2D>("stripes");
            _texFloor = Content.Load<Texture2D>("floor2");
            _texGlobe = Content.Load<Texture2D>("globe3");

            _pillowNormal = Content.Load<Texture2D>("154_norm");
            _pillowColor = Content.Load<Texture2D>("154");
            _globeColor = Content.Load<Texture2D>("154");
            _globeNormal = Content.Load<Texture2D>("154_norm");
            


            _pBatch.ClearGBufferEffect = Content.Load<Effect>("../PrimtiveBatch/Effects/ClearGBuffer.build.fx");
            _pBatch.RenderGBufferEffect = Content.Load<Effect>("../PrimtiveBatch/Effects/RenderGBuffer.build.fx");
            _pBatch.PassThroughEffect = Content.Load<Effect>("../PrimtiveBatch/Effects/PassThrough.build.fx");
            _pBatch.DirectionalLightEffect = Content.Load<Effect>("../PrimtiveBatch/Effects/DirectionalLight.build.fx");
            _pBatch.CombineFinalEffect = Content.Load<Effect>("../PrimtiveBatch/Effects/CombineFinal.build.fx");
            _pBatch.PointLightEffect = Content.Load<Effect>("../PrimtiveBatch/Effects/PointLight.build.fx");
            _pBatch.DepthEffect = Content.Load<Effect>("../PrimtiveBatch/Effects/DepthEffect.build.fx");


            _cam.Target = Vector3.Zero;
            _rand = new Rand();
            _fps = new FPSHelper();



            var ll = new JsonLoader();
            _level = ll.Load("Content\\level.json", Content);

            _plr = new Player();
            _plr.Position = _level.PlayerStart;
            _plr.CubeFront.Texture = _cellTex;
            _plr.CubeFront.Color = Color.White;

            _plr.CubeBack.Texture = _cellTex;
            _plr.CubeBack.Color = Color.White;


            _cam.Position = _level.CameraStart;
            _cam.Target = _level.PlayerStart;
            //_cam.Up = new Vector3(0, 0, 1);

            _model = ObjLoader.Load("Content\\ObjSon\\cow.obj.son");
            

            _cam.Target = Vector3.Zero;
            base.Initialize();
        }


        private float camAngle, camAngle2=.1f, objsAngle, lightAngle, objX;
        
        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            _fps.OnUpdate(gameTime);

            _plr.Update(gameTime);
           // _cam.Update();

            //_cam.Target = _plr.Position;

            if (KeyboardHelper.IsKeyDown(Keys.A))
            {
                camAngle -= .01f;
            }

            if (KeyboardHelper.IsKeyDown(Keys.D))
            {
                camAngle += .01f;
            }

            if (KeyboardHelper.IsKeyDown(Keys.W))
            {
                camAngle2 -= .01f;
            }

            if (KeyboardHelper.IsKeyDown(Keys.S))
            {
                camAngle2 += .01f;
            }

            if (KeyboardHelper.IsKeyDown(Keys.Q))
            {
                objsAngle -= .01f;
            }

            if (KeyboardHelper.IsKeyDown(Keys.E))
            {
                objsAngle += .01f;
            }
            if (KeyboardHelper.IsKeyDown(Keys.O))
            {
                objX -= .01f;
            }

            if (KeyboardHelper.IsKeyDown(Keys.P))
            {
                objX += .01f;
            }

            if (KeyboardHelper.IsKeyDown(Keys.Left))
            {
                var toTarget = new Vector2((float) Math.Cos(camAngle), (float) Math.Sin(camAngle));
                var perp = new Vector2(toTarget.Y, -toTarget.X) ;
                perp.Normalize();
                ;
                _cam.Target -= (new Vector3(perp.X, 0, perp.Y) * .1f);
            }
            if (KeyboardHelper.IsKeyDown(Keys.Right))
            {
                var toTarget = new Vector2((float)Math.Cos(camAngle), (float)Math.Sin(camAngle));
                var perp = new Vector2(toTarget.Y, -toTarget.X) ;
                perp.Normalize();
                ;
                _cam.Target += (new Vector3(perp.X, 0, perp.Y) * .1f);

            }

            if (KeyboardHelper.IsKeyDown(Keys.Up))
            {
                var toTarget = new Vector2((float)Math.Cos(camAngle), (float)Math.Sin(camAngle));
                var perp = toTarget;
                ;
                _cam.Target -= (new Vector3(perp.X, 0, perp.Y) * .1f);
            }
            if (KeyboardHelper.IsKeyDown(Keys.Down))
            {
                var toTarget = new Vector2((float)Math.Cos(camAngle), (float)Math.Sin(camAngle));
                var perp = toTarget;
                ;
                _cam.Target += (new Vector3(perp.X, 0, perp.Y) * .1f);

            }

            lightAngle += .01f;

            KeyboardHelper.Update();
            base.Update(gameTime);
        }



        protected override void Draw(GameTime gameTime)
        {
            _fps.OnDraw();
            
            _pBatch.Begin();

           // _cam.Position = new Vector3(0, 0, -5);
            //_cam.Target = Vector3.Zero;
           

            //_level.Draw(_pBatch);
            //_plr.Draw(_pBatch);
            //_cam.Position = Vector3.Transform(new Vector3(5, 0, 0), Matrix.CreateFromYawPitchRoll(camAngle, 0, camAngle2));
            var r = 3 + objsAngle;
            var rm = (float) Math.Sin(camAngle2);
            var x = (float) Math.Cos(camAngle);
            var z = (float) Math.Sin(camAngle);

            var y = (float) Math.Cos(camAngle2);

            _cam.Position = new Vector3(rm * r*x, r * y, rm * r*z) + _cam.Target;

            var view = _cam.GetView();


            //_cam.Up = new Vector3(0, 0, 0);
            //_cam.Target = Vector3.Zero;
           // _pBatch.Cube(Vector3.Zero + Vector3.UnitZ * b, Vector3.One, Quaternion.CreateFromAxisAngle(Vector3.UnitX, c), _texAgu, Vector2.One, SamplerState.LinearWrap, TextureStyle.PerQuad);


            _pBatch.Cube(new RenderArgs()
            {
                Position = new Vector3(5, -1.4f, 0),
                Size = new Vector3(20, 1, 20),
                Rotation = Quaternion.Identity,
                ColorMap = _texFloor,
                TextureScale = Vector2.One * 5,
                SamplerState = SamplerState.LinearWrap
            });

            //_pBatch.Cube(new Vector3(0, -1.4f, 0), new Vector3(20, 1, 20), Quaternion.Identity, _texFloor, Vector2.One * 5,
            //    SamplerState.LinearWrap);

            _pBatch.Sphere(new RenderArgs()
            {
                Position = new Vector3(2, .2f, 0),
                Rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitY, lightAngle * .2f),
                ColorMap = _globeColor,
                NormalMap = _globeNormal
            });

            //_pBatch.Sphere(new Vector3(objX, .2f, 0), Vector3.One, Quaternion.CreateFromAxisAngle(Vector3.UnitY, lightAngle * .2f), Color.White, _globeColor, _globeNormal);

            //_pBatch.Cube(new RenderArgs()
            //{
            //    Position = new Vector3((float)Math.Cos(lightAngle / 2) * 1.8f, .1f, (float)Math.Sin(lightAngle / 2) * 1.8f),
            //    Rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitX, lightAngle * .3f),
            //    ColorMap = _globeColor,
            //    NormalMap = _globeNormal
            //});

            _pBatch.Model(_model, new RenderArgs()
            {
                Rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitY, lightAngle * .3f),
                Size = Vector3.One * 1.4f,
                Position = new Vector3(-.4f, .5f, 0)
            });

            //_pBatch.Cube(new Vector3((float)Math.Cos(lightAngle / 2) * 1.8f, .1f, (float)Math.Sin(lightAngle / 2) * 1.8f), Vector3.One, Quaternion.CreateFromAxisAngle(Vector3.UnitX, lightAngle * .3f), Color.White, _pillowColor, _pillowNormal, Vector2.One, Vector2.Zero, SamplerState.LinearWrap, TextureStyle.PerQuad);

            //_pBatch.LightPoint(new Vector3(2, 2, 0), Color.Aqua, 3, 1f);
            //_pBatch.LightPoint(new Vector3((float)Math.Cos(lightAngle) * 2, 1, (float)Math.Sin(lightAngle) * 2), Color.LimeGreen, 8f, 1f);

            _pBatch.LightDirectional(new Vector3((float)Math.Cos(lightAngle), 1f, (float)Math.Sin(lightAngle)), Color.LimeGreen);

            _pBatch.Flush(new Color(.1f, .1f, .1f, 1), _cam.Position, view, _cam.ProjectionMatrix);

            var args = new RenderArgs()
            {
                Position = Vector3.Zero
            };

            base.Draw(gameTime);
        }
    }
}
