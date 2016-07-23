using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace DCG.Framework.Util
{
    public class FPSHelper
    {

        public int FrameRate { get; private set; }
        private int _frameCounter = 0;
        private TimeSpan _elapsedTime = TimeSpan.Zero;

        public FPSHelper()
        {
            
        }

        public void OnUpdate(GameTime gameTime)
        {
            _elapsedTime += gameTime.ElapsedGameTime;

            if (_elapsedTime > TimeSpan.FromSeconds(1))
            {
                _elapsedTime -= TimeSpan.FromSeconds(1);
                FrameRate = _frameCounter;
                _frameCounter = 0;
            }
        }

        public void OnDraw()
        {
            _frameCounter++;
        }

    }
}
