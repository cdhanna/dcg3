using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ThreeD
{
    internal struct CustomVertexDeclaration : IVertexType
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

        public CustomVertexDeclaration(Vector3 pos, Color color, Vector2 tex, Vector3 normal)
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

    internal static class CustomVertexFunctions
    {
        internal static List<CustomVertexDeclaration> Concat(this List<CustomVertexDeclaration> self,
            List<CustomVertexDeclaration> other )
        {
            var list = new List<CustomVertexDeclaration>();
            list.AddRange(self);
            list.AddRange(other);
            return list;

        }

        internal static List<CustomVertexDeclaration> SetCapacity(this List<CustomVertexDeclaration> self, int capacity)
        {
            self.Capacity = capacity;
            return self;
        } 

        internal static List<CustomVertexDeclaration> Color(this List<CustomVertexDeclaration> self,
            Color color)
        {
            return self.Select(v => new CustomVertexDeclaration(
                v.Position,
                color,
                v.TextureCoordinate,
                v.Normal)).ToList();

        }


        internal static List<CustomVertexDeclaration> Translate(this List<CustomVertexDeclaration> self,
            Vector3 translate)
        {
            return self.Select(v => new CustomVertexDeclaration(
                v.Position + translate,
                v.Color,
                v.TextureCoordinate,
                v.Normal)).ToList();

        }

        internal static List<CustomVertexDeclaration> Scale(this List<CustomVertexDeclaration> self,
            Vector3 scale)
        {
            return self.Select(v => new CustomVertexDeclaration(
                new Vector3(v.Position.X * scale.X, v.Position.Y * scale.Y, v.Position.Z * scale.Z), 
                v.Color,
                v.TextureCoordinate,
                v.Normal)).ToList();

        }

        internal static List<CustomVertexDeclaration> Rotate(this List<CustomVertexDeclaration> self,
            Vector3 axis, float radians)
        {
            var rotationMatrix = Matrix.CreateFromAxisAngle(axis, radians);
            
            return self.Select(v => new CustomVertexDeclaration(
                Vector3.Transform(v.Position, rotationMatrix),
                v.Color,
                v.TextureCoordinate,
                 Vector3.Transform(v.Normal, rotationMatrix))).ToList();

        }
    }
}
