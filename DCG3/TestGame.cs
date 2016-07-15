using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using DCG3.GameLogic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using ThreeD;
using ThreeD.Menu;
using ThreeD.PrimtiveBatch;
using ThreeD.Util;

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

        private Texture2D _texAgu, _texNuke, _texC, _texAguStrip, _texBorderGlow, _cellTex;
        private Rand _rand;
        private FPSHelper _fps;

        private Level _level;
        private Player _plr;

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


            _pBatch.ClearGBufferEffect = Content.Load<Effect>("../PrimtiveBatch/Effects/ClearGBuffer.build.fx");
            _pBatch.RenderGBufferEffect = Content.Load<Effect>("../PrimtiveBatch/Effects/RenderGBuffer.build.fx");
            _pBatch.PassThroughEffect = Content.Load<Effect>("../PrimtiveBatch/Effects/PassThrough.build.fx");

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

            base.Initialize();
        }

      

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            _fps.OnUpdate(gameTime);

            _plr.Update(gameTime);
            _cam.Update();

            _cam.Target = _plr.Position;

            if (KeyboardHelper.IsKeyDown(Keys.A))
            {
                a -= .01f;
            }

            if (KeyboardHelper.IsKeyDown(Keys.D))
            {
                a += .01f;
            }

            if (KeyboardHelper.IsKeyDown(Keys.W))
            {
                b -= .01f;
            }

            if (KeyboardHelper.IsKeyDown(Keys.S))
            {
                b += .01f;
            }

            if (KeyboardHelper.IsKeyDown(Keys.Q))
            {
                c -= .01f;
            }

            if (KeyboardHelper.IsKeyDown(Keys.E))
            {
                c += .01f;
            }


            KeyboardHelper.Update();
            base.Update(gameTime);
        }

        private float a = 0,  b = 0, c = 0 ;

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.DarkBlue);
            _fps.OnDraw();
            
            _pBatch.Begin();

           // _cam.Position = new Vector3(0, 0, -5);
            //_cam.Target = Vector3.Zero;
            var view = _cam.GetView();



            //_level.Draw(_pBatch);
            //_plr.Draw(_pBatch);
            var r = c + 5;
            _cam.Position = new Vector3(r* (float)Math.Cos(a), 0, r* (float)Math.Sin(a));
            _cam.Target = Vector3.Zero;
           // _pBatch.Cube(Vector3.Zero + Vector3.UnitZ * b, Vector3.One, Quaternion.CreateFromAxisAngle(Vector3.UnitX, c), _texAgu, Vector2.One, SamplerState.LinearWrap, TextureStyle.PerQuad);


            _pBatch.Sphere(Vector3.Zero + Vector3.UnitX * b, Vector3.One, .6f);
            _pBatch.Cube(new Vector3(0, 1, 3), Vector3.One, Quaternion.Identity, _texAgu );
            _pBatch.Sphere(Vector3.Zero + Vector3.UnitY * 2, new Vector3(1, 1, 2),  1);
            //_pBatch.AtlasShown = true;
            _pBatch.Flush(view, _cam.ProjectionMatrix);

            base.Draw(gameTime);
        }
    }
}
