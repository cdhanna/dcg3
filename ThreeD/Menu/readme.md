# Using the Menu system

* Basics
    - The menu system is pretty crappy, but it'll do what it needs to if you coerce it enough. There are currently only **two** base components, a container, and a label. Containers can nest containers or labels. Labels are endpoints in the UI. These two components have been built up to provide some nicer features, like what is called a _datalabel_, or a named value that refreshes every draw call. 
    
* Notes
    - Everything is ratio based. That means that a component with a Size of [1,1] is going to take up the full width and height of its parent. If the component was placed at [.5, .5], then it would be the width and height of its parent, but positioned at the center of its parent. It would look odd. The sizing is a real pain in the asshole.
    
* Usage
    - You can try and smash together the components yourselves by adding things to the _Components_ list of a container, and then making a Renderer, and asking it to draw that container. Or, you can use the _MenuSystem_ for now, which should give you everything you need to pull together some helpful UIs. Here is some code examples of things.


```csharp
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

    public class TestGame : Game
    {
        GraphicsDeviceManager graphics;

        // the _menySystem is the base type you need to start working with the menus. 
        public MenuSystem _menuSystem;

        public TestGame()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {
            
            // sadly, we need to load a SpriteFont. 
            var font = Content.Load<SpriteFont>("basic");

            // and create our system
            _menuSystem = new MenuSystem(GraphicsDevice, font);

            // now we actually set the content. 
            // This is using the 'MenuComposer', which allows you to create and customize a menu with a more 'functional' approach to things. The method takes a variable number of lamdas that take the top level container context as input, and output whatever. You can add components by calling methods on the context as shown. The result of a 'Label' call will be a Label context. At the end of each sub context, you must call .Add(), or it won't be placed in the parent context. 
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
            base.Update(gameTime);
        }


        protected override void Draw(GameTime gameTime)
        {

            _menuSystem.Draw();

            base.Draw(gameTime);
        }
    }
}

```
