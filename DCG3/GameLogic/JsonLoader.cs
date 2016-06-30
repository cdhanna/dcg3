using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;

namespace DCG3.GameLogic
{
    class ObjectJSON
    {
        public float X, Y, Z, Width, Height, Depth, R, G, B;

        public ObjectJSON()
        {
            Width = 1;
            Height = 1;
            Depth = 1;
            R = 255;
            G = 255;
            B = 255;
        }
    }

    class LevelJSON
    {
        public List<ObjectJSON> Objects;

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
                    Position = new Vector3(o.X, o.Y, o.Z),
                    Size = new Vector3(o.Width, o.Height, o.Depth),
                    Color = new Color(o.R, o.G, o.B)
                }
            ).ToList();

            return level;
        }

    }
}
