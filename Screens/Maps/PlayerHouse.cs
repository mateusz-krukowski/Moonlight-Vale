using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Squared.Tiled;

namespace Moonlight_Vale.Screens.Maps;

public class PlayerHouse : IMap
{
    public Map TileMap { get; set; }
    public string PathToTileMap => @"\Tilemaps\player_house.tmx";
    public HashSet<int> PasableTileIds { get; set; }
    public Vector2 PlayerSpawnPoint { get; set; }
    public List<(int, int)> Portals { get; set; }
    private Texture2D TileSet => TileMap.Tilesets.Values.First().Texture;
    public OverworldScreen OverworldScreen { get; }
    

    public PlayerHouse(OverworldScreen overworldScreen)
    {   
        OverworldScreen = overworldScreen;
        PlayerSpawnPoint = new Vector2(8, 13); // Example spawn point, adjust as needed
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

    public void LoadContent(ContentManager content)
    {
        TileMap = Map.Load(content.RootDirectory + PathToTileMap, content);
    }
}