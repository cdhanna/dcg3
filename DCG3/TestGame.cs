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

namespace DCG3
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class TestGame : Game
    {
        GraphicsDeviceManager graphics;

        private IPrimitiveBatch _pBatch;

        public MenuSystem _menuSystem;

        private Level _level;
        private Player _plr;
        private SimpleCamera _cam;

        public TestGame()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {
            _pBatch = new PrimitiveBatch(GraphicsDevice);
            var loader = new JsonLoader();
            _level = loader.Load("Content/level.json");

            _plr = new Player();
            _plr.Position = _level.PlayerStart;

            _cam = new SimpleCamera();
            _cam.Position = _level.CameraStart;


            var font = Content.Load<SpriteFont>("basic");
            _menuSystem = new MenuSystem(GraphicsDevice, font);

            var label = new MenuLabel();
            label.Text = "Hello world";
            label.Color = Color.White;
            label.Position = new Vector2(.01f, .03f);
            label.MinSize = new Vector2(.96f, 0);
            label.Background = new Color(0, 0, 0, .7f);
            label.BorderColor = Color.White;

           

            _menuSystem.Set(

                m => m.Label("Menu Test")
                    .Add(),
                m => m.DataList( 
                    m.KeyValue("Player X", () => _plr.Position.X.ToString()), 
                    m.KeyValue("Player Z", () => _plr.Position.Z.ToString()))
                    .Add(),

                m => m.Do(b => b.Width(.3f))
                );


            base.Initialize();
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            _plr.Update(gameTime);
            _cam.Target = _plr.Position;


            KeyboardHelper.Update();
            base.Update(gameTime);
        }


        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            _pBatch.Begin();

            _level.Draw(_pBatch);
            _plr.Draw(_pBatch);

            _pBatch.Flush(_cam.GetView());

            _menuSystem.Draw();

            base.Draw(gameTime);
        }
    }
}
