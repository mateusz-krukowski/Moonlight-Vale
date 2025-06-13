using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Squared.Tiled;

namespace Moonlight_Vale.Screens.Maps
{
    public class PlayerHouse : IMap
    {
        public Map TileMap { get; set; } //set in LoadContent()
        public string PathToTileMap => @"\Tilemaps\player_house.tmx";
        public HashSet<int> PasableTileIds { get; set; }
        public Vector2 PlayerSpawnPoint { get; set; }
        public List<(int, int)> Portals { get; set; }
        public Texture2D TileSet => TileMap?.Tilesets?.Values?.FirstOrDefault()?.Texture;
        public OverworldScreen OverworldScreen { get; }

        public PlayerHouse(OverworldScreen overworldScreen)
        {   
            OverworldScreen = overworldScreen;
            PlayerSpawnPoint = new Vector2(240,356);
            PasableTileIds = new HashSet<int>
            {
                202, // floor
                200,201, //door
                168, 169, 170, // chair
                121, //another chair
            };
        }

        public void CheckForExitTransition(KeyboardState keyboard, KeyboardState previousKeyboard)
        {
            var player = OverworldScreen.Player;
   
            if (player.Position.X > 219 && player.Position.X < 265 && player.Position.Y == 366)
            {
                if (keyboard.IsKeyDown(Keys.E) && previousKeyboard.IsKeyUp(Keys.E))
                {
                    OverworldScreen.SwitchToFarm();
                }
            }
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
        }
    }
}