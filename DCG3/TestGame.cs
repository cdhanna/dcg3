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

namespace DCG3
{

    struct CubeMeta
    {
        public float YSpeed, AngleSpeed;
    }
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class TestGame : Game
    {
        GraphicsDeviceManager graphics;

        private PrimitiveBatch _pBatch;

        public MenuSystem _menuSystem;

        private SimpleCamera _cam;
        private float _camAngle, _camRadius;

        private Texture2D _texAgu, _texNuke, _texC, _texAguStrip;
        private bool _isRunningSlowly;

        private Random _rand;
        private List<Cube> _cubes;


        int frameRate = 0;
        int frameCounter = 0;
        TimeSpan elapsedTime = TimeSpan.Zero;
       

        private Dictionary<Cube, CubeMeta> _cubeMetas; 

        public TestGame()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {
            _pBatch = new PrimitiveBatch(GraphicsDevice);
            _cam = new SimpleCamera(GraphicsDevice);

            var font = Content.Load<SpriteFont>("basic");
            _menuSystem = new MenuSystem(GraphicsDevice, font);

            _texAgu = Content.Load<Texture2D>("agu");
            _texAguStrip = Content.Load<Texture2D>("agustrip");
            _texNuke = Content.Load<Texture2D>("nuke");
            _texC = Content.Load<Texture2D>("C");

            _menuSystem.Set(
                m => m.Label("Batching Test")
                    .Add(),
                m => m.DataList(
                    m.KeyValue("FPS", () => frameRate.ToString()))
                    .Add(),
                m => m.Do(b => b.Width(.3f)).Do(b => b.X(.7f)).Do(b => b.Height(.5f))
                );

            
            _cam.Target = Vector3.Zero;
            _camRadius = 10;
            _camAngle = 0;

            _rand = new Random();
            
            _cubeMetas = new Dictionary<Cube, CubeMeta>();
            _cubes = new List<Cube>();
            for (var i = 0; i < 10000; i++)
            {
                var c = new Cube();
                c.Texture = _texAgu;
                c.Position = new Vector3(RandomFloat(-7, 7), RandomFloat(-300, 0), RandomFloat(-7, 7));
                c.Rotation = new Rotation(RandomUnit(), RandomFloat(0, MathHelper.Pi));
                c.Size = new Vector3(RandomFloat(1, 2), RandomFloat(1, 2), RandomFloat(1, 2)) * .5f;
                if (RandomCheck())
                {


                    if (RandomCheck())
                        c.Texture = _texNuke;

                    if (RandomCheck())
                    {
                        c.UV = new Vector2(2,3);

                    }
                }
                else
                {
                    //c.Texture = _texAguStrip;
                    //c.TextureStyle = TextureStyle.Wrap;
                }

                var meta = new CubeMeta();
                meta.AngleSpeed = RandomFloat(-.05f, .05f);
                meta.YSpeed = RandomFloat(.02f, .05f);
                _cubeMetas.Add(c, meta);
                _cubes.Add(c);
            }


            GraphicsDevice.DeviceLost += (o, a) =>
            {
                Console.WriteLine("Oh Fuck");
            };

            base.Initialize();
        }

        private bool RandomCheck()
        {
            return _rand.Next()%2 == 0;
        }

        private float RandomFloat(float min, float max)
        {
            var x = min + (_rand.NextDouble()*(max - min));
            return (float) x;
        }

        private Vector3 RandomUnit()
        {
            var theta = RandomFloat(0, MathHelper.TwoPi);
            var z = RandomFloat(-1, 1);

            var z2 = Math.Sqrt(1 - z*z);
            var v = new Vector3((float) (z2*Math.Cos(theta)), (float) (z2*Math.Sign(theta)), z);
            v.Normalize();
            return v;
        }
        

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();


            elapsedTime += gameTime.ElapsedGameTime;

            if (elapsedTime > TimeSpan.FromSeconds(1))
            {
                elapsedTime -= TimeSpan.FromSeconds(1);
                frameRate = frameCounter;
                frameCounter = 0;
            }


            _cubes.ForEach(c =>
            {
                c.Rotation = new Rotation(c.Rotation.Axis, c.Rotation.Radians + _cubeMetas[c].AngleSpeed);
                c.Position -= Vector3.UnitY * _cubeMetas[c].YSpeed;

                if (c.Position.Y < -300)
                {
                    c.Position *= new Vector3(1, 0, 1);
                }
            });

            _isRunningSlowly = gameTime.IsRunningSlowly;


            float camSpeed = .2f;
            if (KeyboardHelper.IsKeyDown(Keys.A))
                _camAngle -= .01f;
            if (KeyboardHelper.IsKeyDown(Keys.D))
                _camAngle += .01f;

            if (KeyboardHelper.IsKeyDown(Keys.Q))
                _camRadius -= .1f;
            if (KeyboardHelper.IsKeyDown(Keys.E))
                _camRadius += .1f;

    
            if (KeyboardHelper.IsKeyDown(Keys.W))
                _cam.Pan(Vector3.UnitY * camSpeed);
            if (KeyboardHelper.IsKeyDown(Keys.S))
                _cam.Pan(Vector3.UnitY * -camSpeed);


            _cam.Position = new Vector3(_camRadius * (float)Math.Cos(_camAngle), _cam.Position.Y, _camRadius * (float)Math.Sin(_camAngle));

            _cam.Update();

            KeyboardHelper.Update();
            base.Update(gameTime);
        }


        protected override void Draw(GameTime gameTime)
        {
            frameCounter++;
            GraphicsDevice.Clear(Color.Black);

            _pBatch.Begin();
            //_cubes.Where(c => c.Position.Y > _cam.Position.Y).toli(c => c.Draw(_pBatch));

            var view = _cam.GetView();
            var bf = new BoundingFrustum(view*_cam.ProjectionMatrix);
            
           
            foreach (var c in _cubes)
            {
                if (bf.Intersects(new BoundingBox(c.Position - c.Size/2, c.Position + c.Size * 2)))
                    c.Draw(_pBatch);
            }

            //_pBatch.Cube(Vector3.Zero, Vector3.One, Rotation.None, Color.Red, _texAgu, Vtor2.One, SamplerState.LinearWrap, TextureStyle.PerQuad);

            _pBatch.Flush(view, _cam.ProjectionMatrix);

            _menuSystem.Draw();

            base.Draw(gameTime);
        }
    }
}
