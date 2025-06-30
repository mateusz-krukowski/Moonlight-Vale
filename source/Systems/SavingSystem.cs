using System;
using System.IO;
using System.IO.Compression;
using System.Text.Json;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;
using System.Linq;
using Moonlight_Vale.Screens.Maps;
using Moonlight_Vale.Entity.Items;
using Moonlight_Vale.Entity;
using Microsoft.Xna.Framework;

namespace Moonlight_Vale.Systems
{
    // Data classes for JSON serialization
    public class PlayerSaveData
    {
        public string Name { get; set; }
        public string CurrentMap { get; set; }
        public PlayerPosition Position { get; set; }
        public List<ItemSaveData> Inventory { get; set; }
        public List<ItemSaveData> ActionBar { get; set; }
        public int Gold { get; set; }
        public int SelectedItem { get; set; }
    }

    public class PlayerPosition
    {
        public float X { get; set; }
        public float Y { get; set; }
    }

    public class ItemSaveData
    {
        public string Name { get; set; }
        public string Type { get; set; } // "Tool", "Seed", "Crop", "Food", "Miscellaneous"
        public int Amount { get; set; }
        public int Price { get; set; }
        public string Description { get; set; }
        public string IconPath { get; set; }
        public int StackSize { get; set; }
        
        // Tool-specific properties
        public int? Durability { get; set; }
        public int? MaxDurability { get; set; }
        public string ToolType { get; set; } // "Shovel", "Hoe", "WateringCan", "Scythe"
        
        // Plant-specific properties (for Seeds and Crops)
        public string PlantName { get; set; } // For recreating seeds/crops
    }

    public class TileChange
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int NewTileId { get; set; }
    }

    public class SavingSystem
    {
        private static SavingSystem _instance;

        public ZipArchive activeSave;
        public string activeSavePath;

        ZipArchiveEntry worldDataEntry;
        ZipArchiveEntry playerDataEntry;
        ZipArchiveEntry farmDataEntry;

        public readonly string savePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Saves");

        public static SavingSystem Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new SavingSystem();
                }
                return _instance;
            }
        }

        private SavingSystem()
        {
            if (!Directory.Exists(savePath))
            {
                Directory.CreateDirectory(savePath);
            }
        }

        public void InitializeNewSave() // Pass player name from IntroScreen later
        {
            try
            {
                var existingSaves = Directory.GetFiles(savePath, "Save_*.save").Length;
                var nextSaveNumber = existingSaves + 1;
                var saveFileName = $"Save_{nextSaveNumber}.save";
                activeSavePath = Path.Combine(savePath, saveFileName);

                // Create default player data for new game
                var playerData = new PlayerSaveData
                {
                    Name = "Player",
                    CurrentMap = "player_farm",
                    Position = new PlayerPosition { X = 950, Y = 700 },
                    Inventory = new List<ItemSaveData>(),
                    ActionBar = new List<ItemSaveData>(),
                    Gold = 150,
                    SelectedItem = 0
                };

                // Initialize empty inventory (30 slots)
                for (int i = 0; i < 30; i++)
                {
                    playerData.Inventory.Add(null);
                }

                // Initialize empty action bar (10 slots)
                for (int i = 0; i < 10; i++)
                {
                    playerData.ActionBar.Add(null);
                }

                var timeData = new { day = 1, hour = 6, minute = 0 };

                string tmxMapPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Content", "Tilemaps", "player_farm_reduced.tmx");
                bool tmxExists = File.Exists(tmxMapPath);
                
                byte[] tmxBytes = null;
                string encodedMap = "";
                
                if (tmxExists)
                {
                    tmxBytes = File.ReadAllBytes(tmxMapPath);
                    encodedMap = Convert.ToBase64String(tmxBytes);
                }

                using (var zipToOpen = new FileStream(activeSavePath, FileMode.Create))
                {
                    using (var archive = new ZipArchive(zipToOpen, ZipArchiveMode.Update))
                    {
                        // Save player data
                        var playerEntry = archive.CreateEntry("player.dat");
                        using (var entryStream = new StreamWriter(playerEntry.Open()))
                        {
                            string json = JsonSerializer.Serialize(playerData, new JsonSerializerOptions { WriteIndented = true });
                            entryStream.Write(json);
                        }

                        // Save time data
                        var timeEntry = archive.CreateEntry("time.dat");
                        using (var entryStream = new StreamWriter(timeEntry.Open()))
                        {
                            string json = JsonSerializer.Serialize(timeData, new JsonSerializerOptions { WriteIndented = true });
                            entryStream.Write(json);
                        }

                        // Save map data
                        if (tmxExists)
                        {
                            var mapEntry = archive.CreateEntry("player_farm_map.dat");
                            using (var entryStream = new StreamWriter(mapEntry.Open()))
                            {
                                entryStream.Write(encodedMap);
                            }
                        }
                    }
                }

                Console.WriteLine($"New save created: {saveFileName}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating save: {ex.Message}");
            }
        }

        public void SavePlayerData(Player player, string currentMapName)
        {
            if (string.IsNullOrEmpty(activeSavePath) || !File.Exists(activeSavePath))
            {
                Console.WriteLine("No active save file to update player data");
                return;
            }

            try
            {
                var playerData = new PlayerSaveData
                {
                    Name = player.Name,
                    CurrentMap = currentMapName,
                    Position = new PlayerPosition { X = player.Position.X, Y = player.Position.Y },
                    Inventory = SerializeInventory(player.Inventory),
                    ActionBar = SerializeInventory(player.ActionBar),
                    Gold = player.Gold,
                    SelectedItem = player.SelectedItem
                };

                // Update player data in save file
                using (var zipToOpen = new FileStream(activeSavePath, FileMode.Open, FileAccess.ReadWrite))
                {
                    using (var archive = new ZipArchive(zipToOpen, ZipArchiveMode.Update))
                    {
                        // Remove existing player entry
                        var existingEntry = archive.GetEntry("player.dat");
                        existingEntry?.Delete();

                        // Create new player entry
                        var playerEntry = archive.CreateEntry("player.dat");
                        using (var entryStream = new StreamWriter(playerEntry.Open()))
                        {
                            string json = JsonSerializer.Serialize(playerData, new JsonSerializerOptions { WriteIndented = true });
                            entryStream.Write(json);
                        }
                    }
                }

                Console.WriteLine("Player data saved successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to save player data: {ex.Message}");
            }
        }

        public PlayerSaveData LoadPlayerData()
        {
            if (string.IsNullOrEmpty(activeSavePath) || !File.Exists(activeSavePath))
            {
                Console.WriteLine("No save file to load player data from");
                return null;
            }

            try
            {
                using (var zipToOpen = new FileStream(activeSavePath, FileMode.Open, FileAccess.Read))
                {
                    using (var archive = new ZipArchive(zipToOpen, ZipArchiveMode.Read))
                    {
                        var playerEntry = archive.GetEntry("player.dat");
                        if (playerEntry == null)
                        {
                            Console.WriteLine("No player.dat found in save file");
                            return null;
                        }

                        using (var entryStream = new StreamReader(playerEntry.Open()))
                        {
                            string json = entryStream.ReadToEnd();
                            var playerData = JsonSerializer.Deserialize<PlayerSaveData>(json);
                            Console.WriteLine("Player data loaded successfully");
                            return playerData;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to load player data: {ex.Message}");
                return null;
            }
        }

        private List<ItemSaveData> SerializeInventory(List<Item> inventory)
        {
            var serializedInventory = new List<ItemSaveData>();

            foreach (var item in inventory)
            {
                if (item == null)
                {
                    serializedInventory.Add(null);
                    continue;
                }

                var itemData = new ItemSaveData
                {
                    Name = item.Name,
                    Type = item.Type.ToString(),
                    Amount = item.Amount,
                    Price = item.Price,
                    Description = item.Description,
                    IconPath = item.IconPath,
                    StackSize = item.StackSize
                };

                // Handle specific item types
                if (item is Tool tool)
                {
                    itemData.Durability = tool.Durability;
                    itemData.MaxDurability = tool.MaxDurability;
                    itemData.ToolType = tool.TypeOfTool.ToString();
                }
                else if (item is Seed seed)
                {
                    // Extract plant name from seed name
                    itemData.PlantName = seed.Name.Replace(" seed", "");
                }
                else if (item is Crop crop)
                {
                    itemData.PlantName = crop.Name;
                }

                serializedInventory.Add(itemData);
            }

            return serializedInventory;
        }

        public List<Item> DeserializeInventory(List<ItemSaveData> serializedInventory)
        {
            var inventory = new List<Item>();

            foreach (var itemData in serializedInventory)
            {
                if (itemData == null)
                {
                    inventory.Add(null);
                    continue;
                }

                Item item = null;

                try
                {
                    // Recreate item based on type
                    switch (itemData.Type)
                    {
                        case "Tool":
                            if (Enum.TryParse<Tool.ToolType>(itemData.ToolType, out var toolType))
                            {
                                item = new Tool(
                                    itemData.Name,
                                    itemData.Description,
                                    itemData.IconPath,
                                    itemData.StackSize,
                                    itemData.Price,
                                    itemData.Durability ?? 100,
                                    toolType
                                );
                            }
                            break;

                        case "Seed":
                            if (!string.IsNullOrEmpty(itemData.PlantName))
                            {
                                item = Seed.CreateSeed(itemData.PlantName);
                            }
                            break;

                        case "Crop":
                            if (!string.IsNullOrEmpty(itemData.PlantName))
                            {
                                item = Crop.CreateCrop(itemData.PlantName);
                            }
                            break;

                        default:
                            Console.WriteLine($"Unknown item type: {itemData.Type}");
                            break;
                    }

                    if (item != null)
                    {
                        item.Amount = itemData.Amount;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to deserialize item {itemData.Name}: {ex.Message}");
                }

                inventory.Add(item);
            }

            return inventory;
        }

        // Existing map-related methods remain unchanged
        public byte[] LoadMapFromSave()
        {
            if (string.IsNullOrEmpty(activeSavePath) || !File.Exists(activeSavePath))
            {
                return null;
            }

            try
            {
                using (var zipToOpen = new FileStream(activeSavePath, FileMode.Open, FileAccess.Read))
                {
                    using (var archive = new ZipArchive(zipToOpen, ZipArchiveMode.Read))
                    {
                        var mapEntry = archive.GetEntry("player_farm_map.dat");
                        if (mapEntry == null)
                        {
                            return null;
                        }

                        using (var entryStream = new StreamReader(mapEntry.Open()))
                        {
                            string encodedMap = entryStream.ReadToEnd();
                            byte[] mapBytes = Convert.FromBase64String(encodedMap);
                            return mapBytes;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to load map from save: {ex.Message}");
                return null;
            }
        }

        public void SaveMapToSave(byte[] mapBytes)
        {
            if (string.IsNullOrEmpty(activeSavePath) || !File.Exists(activeSavePath))
            {
                return;
            }

            try
            {
                string encodedMap = Convert.ToBase64String(mapBytes);

                using (var zipToOpen = new FileStream(activeSavePath, FileMode.Open, FileAccess.ReadWrite))
                {
                    using (var archive = new ZipArchive(zipToOpen, ZipArchiveMode.Update))
                    {
                        var existingEntry = archive.GetEntry("player_farm_map.dat");
                        existingEntry?.Delete();

                        var mapEntry = archive.CreateEntry("player_farm_map.dat");
                        using (var entryStream = new StreamWriter(mapEntry.Open()))
                        {
                            entryStream.Write(encodedMap);
                        }
                    }
                }
                
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to save map: {ex.Message}");
            }
        }

        public void ModifyTileInSavedMap(int x, int y, int newTileId, int mapWidth)
        {
            if (string.IsNullOrEmpty(activeSavePath) || !File.Exists(activeSavePath))
            {
                return;
            }

            try
            {
                // Load current map from save
                byte[] mapBytes = LoadMapFromSave();
                if (mapBytes == null)
                {
                    Console.WriteLine("Failed to load map data for modification");
                    return;
                }

                // Convert to XML string
                string xmlContent = System.Text.Encoding.UTF8.GetString(mapBytes);
                
                // Parse XML
                XDocument doc = XDocument.Parse(xmlContent);
                
                // Find the layer element - should be the first and only one
                var layerElement = doc.Descendants("layer").FirstOrDefault();
                if (layerElement == null)
                {
                    Console.WriteLine("No layer found in TMX file");
                    return;
                }

                // Find the data element with CSV encoding
                var dataElement = layerElement.Descendants("data").FirstOrDefault(d => 
                    d.Attribute("encoding")?.Value == "csv");
                
                if (dataElement == null)
                {
                    Console.WriteLine("No CSV data element found in layer");
                    return;
                }

                // Get CSV content and parse like Squared.Tiled does
                string csvContent = dataElement.Value.Trim();
                var dump = csvContent.Split(',');
                var tileIds = new int[dump.Length];
                
                // Parse each tile ID (handling whitespace like the library does)
                for (int i = 0; i < dump.Length; i++)
                {
                    if (!string.IsNullOrWhiteSpace(dump[i]))
                    {
                        tileIds[i] = int.Parse(dump[i].Trim());
                    }
                    else
                    {
                        tileIds[i] = 0;
                    }
                }

                // Calculate 1D index from 2D coordinates
                int index = y * mapWidth + x;
                
                if (index >= 0 && index < tileIds.Length)
                {
                    // Modify the tile at calculated position
                    tileIds[index] = newTileId;
                    
                    // Convert back to CSV format with proper line breaks
                    var csvLines = new List<string>();
                    int mapHeight = tileIds.Length / mapWidth;
                    
                    for (int row = 0; row < mapHeight; row++)
                    {
                        var rowData = new List<string>();
                        for (int col = 0; col < mapWidth; col++)
                        {
                            int tileIndex = row * mapWidth + col;
                            rowData.Add(tileIds[tileIndex].ToString());
                        }
                        csvLines.Add(string.Join(",", rowData));
                    }
                    string newCsvContent = string.Join(",\n", csvLines);
                    
                    // Update the data element with proper formatting
                    dataElement.Value = "\n" + newCsvContent + "\n";
                    
                    // Convert back to bytes
                    byte[] modifiedMapBytes = System.Text.Encoding.UTF8.GetBytes(doc.ToString());
                    
                    // Save back to archive
                    SaveMapToSave(modifiedMapBytes);
                    
                    Console.WriteLine($"Successfully modified tile at ({x}, {y}) to ID {newTileId}");
                }
                else
                {
                    Console.WriteLine($"Invalid tile coordinates ({x}, {y}) or index {index} out of bounds");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to modify tile in saved map: {ex.Message}");
            }
        }

        public void SaveTimeData(TimeSystem timeSystem)
        {
            
        }
        
        public void LoadSave(string saveFilePath)
        {
            if (!File.Exists(saveFilePath))
            {
                return;
            }

            activeSavePath = saveFilePath;
        }

        public string GetCurrentMapName(IMap currentMap)
        {
            // Helper method to convert map type to string
            return currentMap switch
            {
                PlayerFarm => "player_farm",
                PlayerHouse => "player_house", 
                Town => "town",
                Shop => "shop",
                _ => "player_farm" // Default fallback
            };
        }
    }
}