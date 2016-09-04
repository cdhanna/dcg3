using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Microsoft.Xna.Framework;

namespace DCG.Framework.PrimtiveBatch.ObjLoader
{
    public class ObjLoaderAdvanced
    {

        public DcgModel Load(string objPath)
        {
            var lines = File.ReadAllLines(objPath);

            var positions = new List<Vector4>();
            var textures = new List<Vector3>();
            var normals = new List<Vector3>();

            var model = new DcgModel();
            model.Indicies = new List<uint>();


            var verts = new List<VertexPositionColorNormalTexture>();


            var verticies = new Dictionary<Tuple<int, int, int>, int>();
            var faces = new List<List<Tuple<int, int, int>>>();

            // populate positions, textures, and normals lists.
            for (var i = 0; i < lines.Length; i++)
            {
                var parts = lines[i].Split(' ');
                switch (parts[0])
                {
                    case "v":
                        var pos = new Vector4(
                            float.Parse(parts[1]),
                            float.Parse(parts[2]),
                            float.Parse(parts[3]),
                            parts.Length == 5 ? float.Parse(parts[4]) : 1.0f);
                        positions.Add(pos);

                        break;
                    case "vt":
                        var tex = new Vector3(
                            float.Parse(parts[1]),
                            1 - (parts.Length > 2 ? float.Parse(parts[2]) : 0.0f),
                            parts.Length > 3 ? float.Parse(parts[3]) : 0.0f);

                        textures.Add(tex);
                        break;
                    case "vn":
                        var nom = new Vector3(
                            float.Parse(parts[1]),
                            float.Parse(parts[2]),
                            float.Parse(parts[3]));

                        normals.Add(nom);
                        break;
                    case "f":

                        var face = new List<Tuple<int, int, int>>();

                        for (var p = 1; p < parts.Length; p++)
                        {
                            var keyStr = parts[p];
                            var comps = keyStr.Split('/');

                            var key = new Tuple<int, int, int>(
                                int.Parse(comps[0]),
                                int.Parse(comps[1]),
                                int.Parse(comps[2]));

                            face.Add(key);

                            if (!verticies.ContainsKey(key))
                            {
                                verticies.Add(key, -1);
                            }
                        }

                        faces.Add(face);

                        break;
                    default:
                        break;
                }
            }

                verticies.Keys.ToList().ForEach(key =>
                {
                    var vertPos = positions[key.Item1 - 1];
                    var vertTex = textures[key.Item2 - 1];

                    var vert = new VertexPositionColorNormalTexture(
                        new Vector3(vertPos.X, vertPos.Y, vertPos.Z),
                        Color.White,
                        new Vector2(vertTex.X, vertTex.Y),
                        normals[key.Item3 - 1]);
                    verts.Add(vert);

                    verticies[key] = verts.Count - 1;
                });

                model.Verticies = verts.ToArray();
                
                faces.ForEach(face =>
                {
                    face.ForEach(vertexTuple =>
                    {
                        model.Indicies.Add( (uint) verticies[vertexTuple] );
                    });
                });

            

            return model;
        }

    }
}
