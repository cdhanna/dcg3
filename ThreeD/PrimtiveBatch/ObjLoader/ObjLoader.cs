using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace DCG.Framework.PrimtiveBatch.ObjLoader
{

    public class ObjVertex
    {
        public Vector3 Position;
        public Vector3 Normal;
    }

    public static class ObjLoader
    {
        private const string OBJ_CODE_VERTEX = "v";
        private const string OBJ_CODE_FACE = "f";

        public static DcgModel Load(string objFile)
        {

            /*
             * Vertex
             * v f1 f2 f3
             * 
             * Face
             * f i1 i2 i3 
             */

            //var intRex = @"(-?[1-9]?[0-9]+)";
            //var floatRex = @"(-?[0-9]*(?:\.[0-9]*)?)";

            
            //var fileContent = "v 1.0 2.5 -3.6 \nf 1 24 0";

            //var lines = new string[]
            //{
            //    "v 1.0 2.5 -3.6",
            //    "f 1 2 3"
            //};

            var lines = File.ReadAllLines(objFile);

            var data = new List<ObjVertex>();
            var indicies = new List<short>();


            var largestPoint = Vector3.Zero;

            for (var i = 0; i < lines.Length; i++)
            {
                var line = lines[i];

                var parts = line.Split(' ');
                var command = parts[0];

                switch (command)
                {
                    case OBJ_CODE_VERTEX:

                        var vec = new Vector3(
                            float.Parse(parts[1]),
                            float.Parse(parts[2]),
                            float.Parse(parts[3])
                            );

                        if (vec.Length() > largestPoint.Length())
                        {
                            largestPoint = vec;
                        }

                        data.Add(new ObjVertex()
                        {
                            Position = vec
                        });

                        break;
                    case OBJ_CODE_FACE:


                        for (var t = 2; t < parts.Length - 1; t += 1)
                        {
                            indicies.Add((short)(short.Parse(parts[1]) - 1));
                            indicies.Add((short)(short.Parse(parts[t + 0]) - 1));
                            indicies.Add((short)(short.Parse(parts[t + 1]) - 1));
                        }

                        break;
                    //case "s":
                    //    i++;
                    //    break;
                    default:
                        break; // don't do anything.
                }

            }

            for (var i = 0; i < data.Count; i++)
            {
                data[i].Position /= largestPoint.Length();
            }

            for (var i = 0; i < indicies.Count; i += 3)
            {
                var i1 = indicies[i] ;
                var i2 = indicies[i + 1] ;
                var i3 = indicies[i + 2] ;

                var v0 = data[i2].Position - data[i1].Position;
                var v1 = data[i3].Position - data[i1].Position;

                var normal = Vector3.Cross(v0, v1);
                data[i1].Normal += normal;
                data[i2].Normal += normal;
                data[i3].Normal += normal;

            }


            var declarationData = new VertexPositionColorNormalTexture[data.Count];

            for (var i = 0; i < data.Count; i++)
            {
                data[i].Normal.Normalize();

                declarationData[i] = new VertexPositionColorNormalTexture(data[i].Position, Color.White, Vector2.Zero, data[i].Normal);

            }



            return new DcgModel()
            {
                Indicies = indicies,
                Verticies = declarationData
            };
        }

    }
}
