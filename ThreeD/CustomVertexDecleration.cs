using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DCG.Framework
{
    public struct VertexPositionColorNormalTexture : IVertexType
    {
        public Vector3 Position;
        public Vector3 Normal;
        public Color Color;
        public Vector2 TextureCoordinate;

        public static readonly VertexDeclaration VertexDeclaration
            = new VertexDeclaration(
                new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
                new VertexElement(sizeof(float) * 3, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0),
                new VertexElement((sizeof(float) * 6), VertexElementFormat.Color, VertexElementUsage.Color, 0),

                new VertexElement(sizeof(float) * 6 + sizeof(byte) * 4, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0)
                );

        public VertexPositionColorNormalTexture(Vector3 pos, Color color, Vector2 tex, Vector3 normal)
        {
            Position = pos;
            Color = color;
            Normal = normal;
            TextureCoordinate = tex;
        }

        VertexDeclaration IVertexType.VertexDeclaration
        {
            get { return VertexDeclaration; }
        }
    }


    // random helper methods of lists of vertexpositioncolornormaltextureblahblahs
    internal static class CustomVertexFunctions
    {
        internal static List<VertexPositionColorNormalTexture> Concat(this List<VertexPositionColorNormalTexture> self,
            List<VertexPositionColorNormalTexture> other )
        {
            var list = new List<VertexPositionColorNormalTexture>();
            list.AddRange(self);
            list.AddRange(other);
            return list;

        }


        internal static List<VertexPositionColorNormalTexture> Color(this List<VertexPositionColorNormalTexture> self,
            Color color)
        {


            return self.Select(v => new VertexPositionColorNormalTexture(
                v.Position,
                color,
                v.TextureCoordinate,
                v.Normal)).ToList();

        }


        internal static List<VertexPositionColorNormalTexture> Translate(this List<VertexPositionColorNormalTexture> self,
            Vector3 translate)
        {
            return self.Select(v => new VertexPositionColorNormalTexture(
                v.Position + translate,
                v.Color,
                v.TextureCoordinate,
                v.Normal)).ToList();

        }

        internal static List<VertexPositionColorNormalTexture> Scale(this List<VertexPositionColorNormalTexture> self,
            Vector3 scale)
        {
            return self.Select(v => new VertexPositionColorNormalTexture(
                new Vector3(v.Position.X * scale.X, v.Position.Y * scale.Y, v.Position.Z * scale.Z), 
                v.Color,
                v.TextureCoordinate,
                v.Normal)).ToList();

        }

        internal static List<VertexPositionColorNormalTexture> Rotate(this List<VertexPositionColorNormalTexture> self,
            Vector3 axis, float radians)
        {
            var rotationMatrix = Matrix.CreateFromAxisAngle(axis, radians);
            
            return self.Select(v => new VertexPositionColorNormalTexture(
                Vector3.Transform(v.Position, rotationMatrix),
                v.Color,
                v.TextureCoordinate,
                 Vector3.Transform(v.Normal, rotationMatrix))).ToList();

        }

        //internal static List<VertexPositionColorNormalTexture> ScaleRotateTranslateColor(this List<VertexPositionColorNormalTexture> self,
        //    Vector3 translate, Vector3 scale, Vector3 axis, float radians, Color color)
        //{
        //    var rotationMatrix = Matrix.CreateFromAxisAngle(axis, radians);

        //    return self.Select(v => new VertexPositionColorNormalTexture(
        //        Vector3.Transform(v.Position * scale, rotationMatrix) + translate,
        //        color,
        //        v.TextureCoordinate,
        //         Vector3.Transform(v.Normal, rotationMatrix))).ToList();

        //}



    }
}
