using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;

namespace ThreeD
{
    public class DeferredLighter
    {

        public RenderTarget2D DiffuseTarget { get; private set; }
        public RenderTarget2D NormalTarget { get; private set; }
        public RenderTarget2D DepthTarget { get; private set; }

        public GraphicsDevice Device { get; private set; }

        public DeferredLighter(GraphicsDevice device)
        {
            Device = device;

            var width = device.Viewport.Width;
            var height = device.Viewport.Height;

            DiffuseTarget = new RenderTarget2D(device, width, height, false, SurfaceFormat.Color, DepthFormat.Depth16);
            NormalTarget = new RenderTarget2D(device, width, height, false, SurfaceFormat.Color, DepthFormat.Depth16);
            DepthTarget = new RenderTarget2D(device, width, height, false, SurfaceFormat.Color, DepthFormat.Depth16);
        }


        public void SetGBuffer()
        {
            Device.SetRenderTarget(DiffuseTarget, 0);
            Device.SetRenderTarget(NormalTarget, 1);
            Device.SetRenderTarget(DepthTarget, 2);
        }

        public void ResolveGBuffer()
        {
            RenderTarget2D nul = null; // yes, this is supposed to be null
            Device.SetRenderTarget(nul, 0);
            Device.SetRenderTarget(nul, 1);
            Device.SetRenderTarget(nul, 2);
        }

    }
}
