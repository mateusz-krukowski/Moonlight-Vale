using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Squared.Tiled;
using System.IO;
using System;
using Moonlight_Vale.Systems;

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
            PlayerSpawnPoint = new Vector2(950, 700);
            PasableTileIds =
            [
                1, //grass
                12, //dirt
                114, 115, 116, //stairs
                98, 99, 100, 101, 102, //house porch
                33, 34, 35, 36, 37, 38, 39, 40, //cliff shadows
                13,111,112, 127,128, //sowing plants
                80,79, //growing plants
                95 //grown plants
            ];
        }

        public void LoadContent(ContentManager content)
        {
            try
            {
                if (OverworldScreen.newGame)
                {
                    // New game - use default map
                    TileMap = Map.Load(content.RootDirectory + PathToTileMap, content);
                }
                else
                {
                    // Continue game - load already modified map from save
                    byte[] mapData = SavingSystem.Instance.LoadMapFromSave();
                    
                    if (mapData != null)
                    {
                        // Try to load from memory stream
                        using (var memoryStream = new MemoryStream(mapData))
                        {
                            try
                            {
                                TileMap = Map.Load(memoryStream.ToString(), content);
                            }
                            catch
                            {
                                // Fall back to temp file if Stream loading not supported
                                string tempPath = Path.Combine(Path.GetTempPath(), "temp_farm_map.tmx");
                                File.WriteAllBytes(tempPath, mapData);
                                TileMap = Map.Load(tempPath, content);
                                File.Delete(tempPath);
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine("Failed to load local map, falling back to default");
                        // Fallback to default map if save fails to load
                        TileMap = Map.Load(content.RootDirectory + PathToTileMap, content);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to load map: {ex.Message}");
                if (!OverworldScreen.newGame)
                {
                    Console.WriteLine("Falling back to default map");
                    TileMap = Map.Load(content.RootDirectory + PathToTileMap, content);
                }
                else
                {
                    throw;
                }
            }
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
        

        public void ModifyTile(int x, int y, int newTileId)
        {
            if (TileMap == null)
            {
                return;
            }

            try
            {
                var layer = TileMap.Layers.Values.First();
                
                // Apply the change to the current map in memory
                layer.Tiles[y * layer.Width + x] = newTileId;

                // Immediately save the change to the TMX file in the save
                SavingSystem.Instance.ModifyTileInSavedMap(x, y, newTileId, layer.Width);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to modify tile: {ex.Message}");
            }
        }
    }
}