using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using OpenTK.Graphics.OpenGL;

namespace DCG3.GameLogic
{
    class ObjectJSON
    {

        public Vector3 Position { get; set; }
        public Vector3 Size { get; set; }
        public Vector3 Color { get; set; }
        //public float X, Y, Z, Width, Height, Depth, R, G, B;

        public ObjectJSON()
        {
            Position = Vector3.Zero;
            Size = Vector3.One;
            Color = Vector3.One;
        }
    }

    class LevelJSON
    {
        public List<ObjectJSON> Objects;
        public Vector3 PlayerStart;

        public LevelJSON()
        {
            Objects = new List<ObjectJSON>();
        }
    }

    class JsonLoader
    {

        public Level Load(string levelPath)
        {
            var level = new Level();

            var json = JsonConvert.DeserializeObject<LevelJSON>(File.ReadAllText(levelPath));
            level.Cubes = json.Objects.Select(o =>
                new Cube()
                {
                    Position = o.Position,
                    Size = o.Size,
                    Color = new Color(o.Color.X, o.Color.Y, o.Color.Z)
                }
            ).ToList();
            level.PlayerStart = json.PlayerStart;

            return level;
        }

    }
}
