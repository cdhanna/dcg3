using DCG.Framework.Net;
using DCG.Framework.PrimtiveBatch;
using DCG3.GameLogic;
using DCG3.NetTest;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PlayerNetTest = DCG3.NetTest.Player;

namespace DCG3
{
    public class TestNetGame : Game
    {
        private NetServer _server;
        private NetClient<SomeNetState> _client;
        private GraphicsDeviceManager _graphics;
        private PrimitiveBatch _pBatch;
        private SimpleCamera _cam;

        private InputManager _inputManager;

        private PlayerNetTest _plr;
        private PlayerStateHandler _plrNetHandler;

        public TestNetGame()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            _plr = new PlayerNetTest();
        }

        public TestNetGame(NetServer server) : base()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            this._server = server;

            _plr = new PlayerNetTest();
        }

        public TestNetGame(string host, string port) : base()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            ClientNetManager<SomeNetState> clientNetManager = new ClientNetManager<SomeNetState>();

            _plr = new PlayerNetTest();
            _plrNetHandler = new PlayerStateHandler();
            clientNetManager.AddHandler(_plrNetHandler);

            _client = new NetClient<SomeNetState>(clientNetManager, host, int.Parse(port));
        }

        protected override void OnExiting(object sender, EventArgs args)
        {
            if (_server != null)
            {
                _server.Shutdown();
            }
            base.OnExiting(sender, args);
        }

        protected override void Initialize()
        {
            _inputManager = new InputManager(new KeyboardInput());
            _pBatch = new PrimitiveBatch(GraphicsDevice);
            _pBatch.RenderDebug = false;
            _cam = new SimpleCamera(GraphicsDevice);

            _cam.Position = new Vector3(0, 0, 15);
            _cam.Target = Vector3.Zero;




            _pBatch.ClearGBufferEffect = Content.Load<Effect>("../PrimtiveBatch/Effects/ClearGBuffer.build.fx");
            _pBatch.RenderGBufferEffect = Content.Load<Effect>("../PrimtiveBatch/Effects/RenderGBuffer.build.fx");
            _pBatch.PassThroughEffect = Content.Load<Effect>("../PrimtiveBatch/Effects/PassThrough.build.fx");
            _pBatch.DirectionalLightEffect = Content.Load<Effect>("../PrimtiveBatch/Effects/DirectionalLight.build.fx");
            _pBatch.CombineFinalEffect = Content.Load<Effect>("../PrimtiveBatch/Effects/CombineFinal.build.fx");
            _pBatch.PointLightEffect = Content.Load<Effect>("../PrimtiveBatch/Effects/PointLight.build.fx");
            _pBatch.DepthEffect = Content.Load<Effect>("../PrimtiveBatch/Effects/DepthEffect.build.fx");

            
            base.Initialize();
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            var inputCollection = _inputManager.Update();

            if (_client != null)
            {
                _client.QueueMessage(inputCollection);
                _plr.Tick(inputCollection);
            } else
            {
                _plr.Tick(inputCollection);
            }
            
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Orange);

            _pBatch.Begin();

            _pBatch.Cube(new RenderArgs()
            {
                Position = _plr.Position,
                Rotation = Quaternion.CreateFromRotationMatrix(
                    Matrix.Identity * Matrix.CreateRotationZ(-_plr.Velocity.X/5f)),
                Color = Color.Red
            });

            _pBatch.LightPoint(new Vector3(2, 0, 0), Color.White, 40, 1);
            _pBatch.LightPoint(new Vector3(-2, 0, 0), Color.White, 40, 1);
            _pBatch.LightPoint(new Vector3(-2, 0, 2), Color.White, 40, 1);
            _pBatch.LightPoint(new Vector3(-2, 0, -2), Color.White, 40, 1);

            _pBatch.Flush(Color.White, _cam.Position, _cam.GetView(), _cam.ProjectionMatrix);

            base.Draw(gameTime);
        }

    }
}
