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
        private const string OBJ_CODE_VERTEX_TEXTURE = "vt";

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
            var indicies = new List<uint>();

            var textureCoords = new List<Vector3>();

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

                    case OBJ_CODE_VERTEX_TEXTURE:

                        var uvw = new[] { 0f, 0f, 0f };
                        var uvwIndex = 0;
                        for (var t = 1; t < parts.Length; t++)
                        {
                            var value = float.Parse(parts[i]);
                            uvw[uvwIndex] = value;
                            value += 1;
                        }
                        textureCoords.Add(new Vector3(uvw[0], uvw[1], uvw[2]));

                        break;
                    case OBJ_CODE_FACE:

                        // need to check if the face command was written like
                        // * f 1 2 3       : f v v v 
                        // * f 1/1 2/2 3/3 : f v/vt v/vt v/vt

                        if (parts[1].Contains("/"))
                        {

                        } else
                        {
                            for (var t = 2; t < parts.Length - 1; t += 1)
                            {
                                indicies.Add((uint)(uint.Parse(parts[1]) - 1));
                                indicies.Add((uint)(uint.Parse(parts[t + 0]) - 1));
                                indicies.Add((uint)(uint.Parse(parts[t + 1]) - 1));
                            }
                        }

                       

                        break;
                    //case "s":
                    //    i++;
                    //    break;
                    default:
                        break; // don't do anything.
                }

            }

            // do we want to do this? Scale to 1
            for (var i = 0; i < data.Count; i++)
            {
                data[i].Position /= largestPoint.Length();
            }

            for (var i = 0; i < indicies.Count; i += 3)
            {
                var i1 = indicies[i] ;
                var i2 = indicies[i + 1] ;
                var i3 = indicies[i + 2] ;

                var v0 = data[(int)i2].Position - data[(int)i1].Position;
                var v1 = data[(int)i3].Position - data[(int)i1].Position;

                var normal = Vector3.Cross(v0, v1);
                data[(int)i1].Normal += normal;
                data[(int)i2].Normal += normal;
                data[(int)i3].Normal += normal;

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
