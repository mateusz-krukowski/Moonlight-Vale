using System;
using System.IO;
using System.IO.Compression;
using System.Text.Json;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;
using System.Linq;
using Moonlight_Vale.Screens.Maps;

namespace Moonlight_Vale.Systems
{
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

        public void InitializeNewSave() //pass playername from IntroScreen
        {
            try
            {
                var existingSaves = Directory.GetFiles(savePath, "Save_*.save").Length;
                var nextSaveNumber = existingSaves + 1;
                var saveFileName = $"Save_{nextSaveNumber}.save";
                activeSavePath = Path.Combine(savePath, saveFileName);

                var playerData = new { Name = "Player", currentMap = "player_farm", Position = new { X = 950, Y = 700 }, Inventory = new List<string>(30), Gold = 100 };
                var timeData = new { day = 1, hour = 6, minute = 0};

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
                        var playerEntry = archive.CreateEntry("player.dat");
                        using (var entryStream = new StreamWriter(playerEntry.Open()))
                        {
                            string json = JsonSerializer.Serialize(playerData, new JsonSerializerOptions { WriteIndented = true });
                            entryStream.Write(json);
                        }

                        var timeEntry = archive.CreateEntry("time.dat");

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
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating save: {ex.Message}");
            }
        }

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

        public void SaveDataAboutPlayer()
        {
            //name
            //current map
            //position
            //inventory
            //gold
            
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
    }
}