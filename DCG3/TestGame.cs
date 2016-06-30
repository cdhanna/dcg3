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
        private KeyboardHelper _kb;


        private Level _level;

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

            base.Initialize();
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            base.Update(gameTime);
        }


        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            _pBatch.Begin(GraphicsDevice);

            _level.Draw(_pBatch);
            //_pBatch.Draw(new Vector3(0, 0, 0), );


            _pBatch.Flush(Matrix.Identity);

            base.Draw(gameTime);
        }
    }
}
