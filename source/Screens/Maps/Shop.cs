using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Moonlight_Vale.Entity;
using Squared.Tiled;

namespace Moonlight_Vale.Screens.Maps;

public class Shop : IMap
{
    public Map TileMap { get; private set; }
    public string PathToTileMap => @"\Tilemaps\shop.tmx";
    public HashSet<int> PasableTileIds { get; set; }
    public Vector2 PlayerSpawnPoint { get; set; }
    public List<(int, int)> Portals { get; set; }
    public OverworldScreen OverworldScreen { get; }
    
    public Texture2D TileSet => TileMap?.Tilesets?.Values?.FirstOrDefault()?.Texture;
    
    public Npc Shopkeeper { get; private set; } //3,9 128, 308????

    public Shop(OverworldScreen overworldScreen)
    {
        OverworldScreen = overworldScreen;
        PlayerSpawnPoint = new Vector2(240,356);
        PasableTileIds = new HashSet<int>
        {
            202, // floor
            200,201, //door
        };
        Shopkeeper = new NpcBuilder<Vendor>()
            .SetName("Amanda the Plant Merchant")
            .SetGreetings([])
            .SetBeforeTradeDialogues([])
            .SetAfterTradeDialogues([])
            .SetFarewells([])
            .SetPosition(new(97,308))
            .Build();
    }
    public void DrawMap(SpriteBatch spriteBatch)
    {
        if (TileMap?.Tilesets == null || !TileMap.Tilesets.Any())
            return;

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
        Shopkeeper?.Draw(spriteBatch);
    }

    public void LoadContent(ContentManager content)
    {
        try
        {
            TileMap = Map.Load(content.RootDirectory + PathToTileMap, content);
        }
        catch (System.Exception ex)
        {
            System.Console.WriteLine($"Failed to load PlayerHouse map: {ex.Message} although it is located in Content/Tilemaps xD");
            TileMap = null;
        }
        Shopkeeper?.LoadContent(content,@"Spritesheets\\seed_vendor");
    }
}