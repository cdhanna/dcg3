using System.Runtime.InteropServices;
using DCG3.GameLogic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace DCG3
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class TestGame : Game
    {
        GraphicsDeviceManager graphics;

        private IPrimitiveBatch _pBatch;



        private Level _level;
        private Player _plr;

        public TestGame()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {
            _pBatch = new FakePBatch();

            var loader = new JsonLoader();
            _level = loader.Load("Content/level.json");

            _plr = new Player();
            _plr.Position = _level.PlayerStart;

            base.Initialize();
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            _plr.Update(gameTime);

            base.Update(gameTime);
        }


        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            _pBatch.Begin(GraphicsDevice);

            _level.Draw(_pBatch);
            _plr.Draw(_pBatch);

            _pBatch.Flush(Matrix.Identity);

            base.Draw(gameTime);
        }
    }
}
