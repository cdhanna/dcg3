using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ThreeD
{
    public class PrimitiveBatch : IPrimitiveBatch
    {
        Matrix projectionMatrix; // camera's lens
        Matrix worldMatrix;      // object's position

        VertexBuffer vertexBuffer;  // use to draw to graphics card
        VertexPositionColor[] allCubes;
        VertexPositionColor[] colorCubeVertices;
        List<VertexPositionColor> allCubesList;

        GraphicsDevice GraphicsDevice;

        BasicEffect basicEffect;

        public PrimitiveBatch( GraphicsDevice graphicsDevice)
        {
            GraphicsDevice = graphicsDevice;

            projectionMatrix = Matrix.CreatePerspectiveFieldOfView(
                MathHelper.ToRadians(45f),
                GraphicsDevice.DisplayMode.AspectRatio, 1f, 10000f);

            worldMatrix = Matrix.CreateWorld(Vector3.Zero, Vector3.Forward, Vector3.Up);

            basicEffect = new BasicEffect(GraphicsDevice);
            basicEffect.Alpha = 1.0f;
            basicEffect.VertexColorEnabled = true;
            basicEffect.LightingEnabled = false;
        }

        public void Begin()
        {
            allCubesList = new List<VertexPositionColor>();
        }

        public void Draw(Vector3 position, float width, float height, float depth, Color color)
        {
            VertexPositionColor[] cubeVertices = GetCubeVertices(position.X, position.Y, position.Z, width, height, depth, color);
            VertexPositionColor[] cubeTriangulated = ConstructCube(cubeVertices);

            allCubesList.AddRange(cubeTriangulated);
        }

        public void Draw(Vector3 position, float width, float height, float depth, Color[] colors)
        {
            VertexPositionColor[] cubeVertices = GetCubeVertices(position.X, position.Y, position.Z, width, height, depth, Color.Black);
            VertexPositionColor[] cubeTriangulated = ConstructCube(cubeVertices, colors);

            allCubesList.AddRange(cubeTriangulated);
        }

        public void Flush(Matrix viewMatrix)
        {
            allCubes = allCubesList.ToArray();
            vertexBuffer = new VertexBuffer(GraphicsDevice, typeof(VertexPositionColor), allCubes.Length, BufferUsage.WriteOnly);
            vertexBuffer.SetData<VertexPositionColor>(allCubes);

            basicEffect.Projection = projectionMatrix;
            basicEffect.View = viewMatrix;
            basicEffect.World = worldMatrix;

            GraphicsDevice.Clear(Color.White);

            GraphicsDevice.SetVertexBuffer(vertexBuffer);

            // Turn off back face culling
            RasterizerState rasterizerState = new RasterizerState();
            rasterizerState.CullMode = CullMode.None;
            GraphicsDevice.RasterizerState = rasterizerState;

            foreach (EffectPass pass in basicEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, allCubes.Length / 3);
            }
        }

        private VertexPositionColor[] GetCubeVertices(float x, float y, float z, float lengthX, float lengthY, float lengthZ)
        {
            VertexPositionColor[] cube = new VertexPositionColor[8];

            cube[0] = new VertexPositionColor(new Vector3(x + lengthX / 2, y + lengthY / 2, z + lengthZ / 2), Color.Red);
            cube[1] = new VertexPositionColor(new Vector3(x + lengthX / 2, y + lengthY / 2, z - lengthZ / 2), Color.Orange);
            cube[2] = new VertexPositionColor(new Vector3(x + lengthX / 2, y - lengthY / 2, z + lengthZ / 2), Color.Yellow);
            cube[3] = new VertexPositionColor(new Vector3(x + lengthX / 2, y - lengthY / 2, z - lengthZ / 2), Color.Green);
            cube[4] = new VertexPositionColor(new Vector3(x - lengthX / 2, y + lengthY / 2, z + lengthZ / 2), Color.Teal);
            cube[5] = new VertexPositionColor(new Vector3(x - lengthX / 2, y + lengthY / 2, z - lengthZ / 2), Color.Blue);
            cube[6] = new VertexPositionColor(new Vector3(x - lengthX / 2, y - lengthY / 2, z + lengthZ / 2), Color.Purple);
            cube[7] = new VertexPositionColor(new Vector3(x - lengthX / 2, y - lengthY / 2, z - lengthZ / 2), Color.DarkRed);

            return cube;
        }

        private VertexPositionColor[] GetCubeVertices(float x, float y, float z, float lengthX, float lengthY, float lengthZ, Color color)
        {
            VertexPositionColor[] cube = new VertexPositionColor[8];

            cube[0] = new VertexPositionColor(new Vector3(x + lengthX / 2, y + lengthY / 2, z + lengthZ / 2), color);
            cube[1] = new VertexPositionColor(new Vector3(x + lengthX / 2, y + lengthY / 2, z - lengthZ / 2), color);
            cube[2] = new VertexPositionColor(new Vector3(x + lengthX / 2, y - lengthY / 2, z + lengthZ / 2), color);
            cube[3] = new VertexPositionColor(new Vector3(x + lengthX / 2, y - lengthY / 2, z - lengthZ / 2), color);
            cube[4] = new VertexPositionColor(new Vector3(x - lengthX / 2, y + lengthY / 2, z + lengthZ / 2), color);
            cube[5] = new VertexPositionColor(new Vector3(x - lengthX / 2, y + lengthY / 2, z - lengthZ / 2), color);
            cube[6] = new VertexPositionColor(new Vector3(x - lengthX / 2, y - lengthY / 2, z + lengthZ / 2), color);
            cube[7] = new VertexPositionColor(new Vector3(x - lengthX / 2, y - lengthY / 2, z - lengthZ / 2), color);

            return cube;
        }

        private VertexPositionColor[] ConstructCube(VertexPositionColor[] vertices)
        {
            VertexPositionColor[] cubeTriangulated = new VertexPositionColor[36];

            cubeTriangulated[0] = vertices[0];
            cubeTriangulated[1] = vertices[1];
            cubeTriangulated[2] = vertices[3];

            cubeTriangulated[3] = vertices[0];
            cubeTriangulated[4] = vertices[2];
            cubeTriangulated[5] = vertices[3];

            cubeTriangulated[6] = vertices[0];
            cubeTriangulated[7] = vertices[1];
            cubeTriangulated[8] = vertices[5];

            cubeTriangulated[9] = vertices[0];
            cubeTriangulated[10] = vertices[4];
            cubeTriangulated[11] = vertices[5];

            cubeTriangulated[12] = vertices[0];
            cubeTriangulated[13] = vertices[2];
            cubeTriangulated[14] = vertices[6];

            cubeTriangulated[15] = vertices[0];
            cubeTriangulated[16] = vertices[4];
            cubeTriangulated[17] = vertices[6];

            cubeTriangulated[18] = vertices[7];
            cubeTriangulated[19] = vertices[6];
            cubeTriangulated[20] = vertices[2];

            cubeTriangulated[21] = vertices[7];
            cubeTriangulated[22] = vertices[3];
            cubeTriangulated[23] = vertices[2];

            cubeTriangulated[24] = vertices[7];
            cubeTriangulated[25] = vertices[6];
            cubeTriangulated[26] = vertices[4];

            cubeTriangulated[27] = vertices[7];
            cubeTriangulated[28] = vertices[5];
            cubeTriangulated[29] = vertices[4];

            cubeTriangulated[30] = vertices[7];
            cubeTriangulated[31] = vertices[5];
            cubeTriangulated[32] = vertices[1];

            cubeTriangulated[33] = vertices[7];
            cubeTriangulated[34] = vertices[3];
            cubeTriangulated[35] = vertices[1];

            return cubeTriangulated;
        }

        private VertexPositionColor[] ConstructCube(VertexPositionColor[] vertices, Color[] colors)
        {
            VertexPositionColor[] cubeTriangulated = new VertexPositionColor[36];

            cubeTriangulated[0] = vertices[0];
            cubeTriangulated[1] = vertices[1];
            cubeTriangulated[2] = vertices[3];

            cubeTriangulated[3] = vertices[0];
            cubeTriangulated[4] = vertices[2];
            cubeTriangulated[5] = vertices[3];

            cubeTriangulated[6] = vertices[0];
            cubeTriangulated[7] = vertices[1];
            cubeTriangulated[8] = vertices[5];

            cubeTriangulated[9] = vertices[0];
            cubeTriangulated[10] = vertices[4];
            cubeTriangulated[11] = vertices[5];

            cubeTriangulated[12] = vertices[0];
            cubeTriangulated[13] = vertices[2];
            cubeTriangulated[14] = vertices[6];

            cubeTriangulated[15] = vertices[0];
            cubeTriangulated[16] = vertices[4];
            cubeTriangulated[17] = vertices[6];

            cubeTriangulated[18] = vertices[7];
            cubeTriangulated[19] = vertices[6];
            cubeTriangulated[20] = vertices[2];

            cubeTriangulated[21] = vertices[7];
            cubeTriangulated[22] = vertices[3];
            cubeTriangulated[23] = vertices[2];

            cubeTriangulated[24] = vertices[7];
            cubeTriangulated[25] = vertices[6];
            cubeTriangulated[26] = vertices[4];

            cubeTriangulated[27] = vertices[7];
            cubeTriangulated[28] = vertices[5];
            cubeTriangulated[29] = vertices[4];

            cubeTriangulated[30] = vertices[7];
            cubeTriangulated[31] = vertices[5];
            cubeTriangulated[32] = vertices[1];

            cubeTriangulated[33] = vertices[7];
            cubeTriangulated[34] = vertices[3];
            cubeTriangulated[35] = vertices[1];

            for (int i = 0; i < 36; i++)
            {
                cubeTriangulated[i].Color = colors[i / 6];
            }

            return cubeTriangulated;
        }

    }
}
