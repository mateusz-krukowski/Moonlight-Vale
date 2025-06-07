using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Squared.Tiled;

namespace Moonlight_Vale.Screens.Maps
{
    public class PlayerFarm : IMap
    {
        public Map TileMap { get; private set; }
        public string PathToTileMap => @"\Tilemaps\player_farm_reduced.tmx";
        public HashSet<int> PasableTileIds { get; set; }
        public (int, int) PlayerSpawnPoint { get; set; }
        public List<(int, int)> Portals { get; set; }
        public OverworldScreen OverworldScreen { get; }

        private Texture2D TileSet => TileMap.Tilesets.Values.First().Texture;
        
        

        public PlayerFarm(OverworldScreen overworldScreen)
        {
            OverworldScreen = overworldScreen;
        }

        public void LoadContent(ContentManager content)
        {
            TileMap = Map.Load(content.RootDirectory  + PathToTileMap, content);
        }

        public void DrawMap(SpriteBatch spriteBatch)
        {
            int scaledTileSize = (int)(TileMap.TileWidth * OverworldScreen.Zoom);

            foreach (var layer in TileMap.Layers.Values)
            {
                for (int y = 0; y < layer.Height; y++)
                {
                    for (int x = 0; x < layer.Width; x++)
                    {
                        int tileIndex = layer.GetTile(x, y);
                        var tileRect = new Rectangle();
                        if (!TileMap.Tilesets.First().Value.MapTileToRect(tileIndex, ref tileRect)) continue;

                        spriteBatch.Draw(TileSet, new Vector2(x, y) * scaledTileSize, tileRect,
                            Color.White, 0, Vector2.Zero, OverworldScreen.Zoom, SpriteEffects.None, 0);
                    }
                }
            }
        }
    }
}