﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;

namespace DCG3.GameLogic
{
    class ObjectJSON
    {

        public Vector3 Position { get; set; }
        public Vector3 Size { get; set; }
        public Vector3 Color { get; set; }
        public string Texture { get; set; }
        public Vector2 TextureTiles { get; set; }
        
        //public float X, Y, Z, Width, Height, Depth, R, G, B;

        public ObjectJSON()
        {
            Position = Vector3.Zero;
            Size = Vector3.One;
            Color = Vector3.One;
            Texture = null;
            TextureTiles = Vector2.One;
        }
    }

    class LevelJSON
    {
        public List<ObjectJSON> Objects;
        public Vector3 PlayerStart;
        public Vector3 CameraStart;

        public LevelJSON()
        {
            Objects = new List<ObjectJSON>();
            PlayerStart = Vector3.Zero;
            CameraStart = new Vector3(0, 5, -5);
        }
    }

    class JsonLoader
    {

        public Level Load(string levelPath, ContentManager content)
        {
            var level = new Level();

            var json = JsonConvert.DeserializeObject<LevelJSON>(File.ReadAllText(levelPath));
            level.Cubes = json.Objects.Select(o =>
                new Cube()
                {
                    Position = o.Position,
                    Size = o.Size,
                    Color = new Color(o.Color.X, o.Color.Y, o.Color.Z),
                    Texture = o.Texture == null ? null : content.Load<Texture2D>(o.Texture),
                    UV = o.TextureTiles
                }
            ).ToList();
            level.PlayerStart = json.PlayerStart;
            level.CameraStart = json.CameraStart;

            return level;
        }

    }
}
