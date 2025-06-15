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
        public Vector2 PlayerSpawnPoint { get; set; }
        public List<(int, int)> Portals { get; set; }
        public OverworldScreen OverworldScreen { get; }

        public Texture2D TileSet => TileMap.Tilesets.Values.First().Texture;

        public PlayerFarm(OverworldScreen overworldScreen)
        {
            OverworldScreen = overworldScreen;
            PlayerSpawnPoint = new Vector2(950,700); // Example spawn point, adjust as needed
            PasableTileIds = new HashSet<int>
            {
                1, //grass
                12,13,127, //dirt
                114,115,116, //stairs
                98,99,100,101,102, //house porch
                33,34,35,36,37,38,39,40 //cliff shadows
                
            }; 

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