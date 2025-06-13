using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Squared.Tiled;

namespace Moonlight_Vale.Screens.Maps
{
    public interface IMap
    {
    
        public Map TileMap { get; }
        public String PathToTileMap { get;  }
        public HashSet<int> PasableTileIds { get; set; }
        public Vector2 PlayerSpawnPoint { get; set; }
        public List<ValueTuple<int,int>> Portals { get; set; }
        public Texture2D TileSet => TileMap.Tilesets.Values.First().Texture;
        public OverworldScreen OverworldScreen { get; }

        public void DrawMap(SpriteBatch spriteBatch);
        public void LoadContent(Microsoft.Xna.Framework.Content.ContentManager content);
    }
}