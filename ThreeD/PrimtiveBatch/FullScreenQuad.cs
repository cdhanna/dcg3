using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DCG.Framework.PrimtiveBatch
{
    class FullScreenQuad
    {

        //Vertex Buffer
        VertexBuffer vb;
        //Index Buffer
        IndexBuffer ib;
        //Constructor

        private VertexPositionTexture[] verts;

        public FullScreenQuad(GraphicsDevice GraphicsDevice)
        {
            //Vertices
            verts = new VertexPositionTexture[]
                 {
                 new VertexPositionTexture(new Vector3(1, -1, 0), new Vector2(1, 1)),
                 new VertexPositionTexture(new Vector3(-1, -1, 0), new Vector2(0, 1)),
                 new VertexPositionTexture(new Vector3(-1, 1, 0), new Vector2(0, 0)),
                 new VertexPositionTexture(new Vector3(1, 1, 0), new Vector2(1, 0))
                 };
            //Make Vertex Buffer
            vb = new VertexBuffer(GraphicsDevice, VertexPositionTexture.VertexDeclaration,
            verts.Length, BufferUsage.None);
            vb.SetData<VertexPositionTexture>(verts);
            //Indices
            ushort[] indices = { 0, 1, 2, 2, 3, 0 };
            //Make Index Buffer
            ib = new IndexBuffer(GraphicsDevice, IndexElementSize.SixteenBits,
           indices.Length, BufferUsage.None);
            ib.SetData<ushort>(indices);
        }
        //Draw and Set Buffers
        public void Draw(GraphicsDevice GraphicsDevice, Vector2? offset=null, Vector2? scale=null)
        {

            if (offset.HasValue || scale.HasValue)
            {
                var offsetVal = offset.HasValue ? offset.Value : Vector2.Zero;
                var scaleVal = scale.HasValue ? scale.Value : Vector2.One;

                var transformed = new VertexPositionTexture[verts.Length];
                for (var i = 0; i < verts.Length; i++)
                {
                    var vert = verts[i];
                    transformed[i] = new VertexPositionTexture(
                        vert.Position*new Vector3(scaleVal, 1f) + new Vector3(offsetVal, 0),
                        vert.TextureCoordinate);
                }
                vb.SetData(transformed);
            }
            else
            {
                vb.SetData(verts);
            }

            //Set Vertex Buffer
            GraphicsDevice.SetVertexBuffer(vb);
            //Set Index Buffer
            GraphicsDevice.Indices = ib;
            //Draw Quad
            //GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, 2);
            GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 4, 0, 2);
        }
        //Set Buffers Onto GPU
        public void ReadyBuffers(GraphicsDevice GraphicsDevice)
        {
            //Set Vertex Buffer
            GraphicsDevice.SetVertexBuffer(vb);
            //Set Index Buffer
            GraphicsDevice.Indices = ib;
        }

        //Draw without Setting Buffers
        public void JustDraw(GraphicsDevice GraphicsDevice)
        {
            //Draw Quad
            GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 4, 0, 2);
        }
    }
}
