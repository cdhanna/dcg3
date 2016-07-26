using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using DCG.Framework.Physics;
using DCG.Framework.Physics.Bodies;
using DCG.Framework.Physics.Colliders;
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
        
        private Rand _rand;
        private FPSHelper _fps;

        private Level _level;
        private Player _plr;

        private Sphere _sphere;
        private Box _box;
        private PlaneBody _planeBody;
        private PhysicsSimulation _sim;

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

            _pillowNormal = Content.Load<Texture2D>("pillow_normal");
            _pillowColor = Content.Load<Texture2D>("pillow_color");
            


            _pBatch.ClearGBufferEffect = Content.Load<Effect>("../PrimtiveBatch/Effects/ClearGBuffer.build.fx");
            _pBatch.RenderGBufferEffect = Content.Load<Effect>("../PrimtiveBatch/Effects/RenderGBuffer.build.fx");
            _pBatch.PassThroughEffect = Content.Load<Effect>("../PrimtiveBatch/Effects/PassThrough.build.fx");
            _pBatch.DirectionalLightEffect = Content.Load<Effect>("../PrimtiveBatch/Effects/DirectionalLight.build.fx");
            _pBatch.CombineFinalEffect = Content.Load<Effect>("../PrimtiveBatch/Effects/CombineFinal.build.fx");
            _pBatch.PointLightEffect = Content.Load<Effect>("../PrimtiveBatch/Effects/PointLight.build.fx");

            _sim = new PhysicsSimulation();;
            _sim.Gravity = new Vector3(0, 0, 0);

            _sphere = new Sphere();
            _sphere.Position = new Vector3(0, 3, 0);

            _box = new Box();
            _box.Position = new Vector3(-2, 3, -1);
            _box.Body.Rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitZ, .6f);
            _box.Body.SigmaForce += new Vector3(0, -.01f, 0);
            _box.Body.Size = new Vector3(3, 1, 1);

            _planeBody = new PlaneBody(Vector3.UnitY, -.5f);

            //_sim.Bodies.Add(_sphere.Body);
            _sim.Bodies.Add(_box.Body);
            _sim.Bodies.Add(_planeBody);


            _rand = new Rand();
            _fps = new FPSHelper();





            _cam.Target = new Vector3(0, 0, 0);
            base.Initialize();
        }


        private float camAngle = 1.57f, camAngle2=1.57f, objsAngle = 3, lightAngle, objX = 3;
        
        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            _fps.OnUpdate(gameTime);


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
            lightAngle += .01f;


            //_sphere.Position = new Vector3(_sphere.Position.X, objX, _sphere.Position.Z);
            //_sphere.Update();

            _sim.Update(gameTime);
            //_sphere.Update();

            //if (_planeCollider.CheckCollision(_sphere.Collider).AnyContact)
            //{
            //    _sphere.Color = Color.Red;
            //}
            //else
            //{
            //    _sphere.Color = Color.White;
            //}

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


            _pBatch.Cube(new Vector3(0, -1f, 0), new Vector3(20, 1, 20), Quaternion.Identity, _texFloor, Vector2.One * 5,
                SamplerState.LinearWrap);

            //_pBatch.Sphere(new Vector3(objX, .2f, 0), Vector3.One, Quaternion.CreateFromAxisAngle(Vector3.UnitY, lightAngle * .2f), Color.White, _texGlobe );

            //_pBatch.Cube(new Vector3(objX, .2f, 0), Vector3.One, Quaternion.Identity, Color.White, _pillowColor,_pillowNormal,Vector2.One,Vector2.Zero, SamplerState.LinearWrap, TextureStyle.PerQuad);

            _box.Draw(_pBatch);

            //_pBatch.LightPoint(new Vector3(0, 2, 0), Color.DimGray, 5, 1f );
            _pBatch.LightPoint(new Vector3((float)Math.Cos(lightAngle) * 2, 1, (float)Math.Sin(lightAngle)*2), Color.Red, 8f, 1f );

            _pBatch.Flush(new Color(.4f, .4f, .4f, 1), _cam.Position, view, _cam.ProjectionMatrix);

            base.Draw(gameTime);
        }
    }
}
