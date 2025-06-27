using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Moonlight_Vale.Entity;
using Moonlight_Vale.Entity.Items;
using Squared.Tiled;

namespace Moonlight_Vale.Screens.Maps
{
    public class Town : IMap
    {
        public Map TileMap { get; set; }
        public string PathToTileMap => @"\Tilemaps\village.tmx";
        public HashSet<int> PasableTileIds { get; set; }
        public Vector2 PlayerSpawnPoint { get; set; }
        public List<(int, int)> Portals { get; set; }
        public Texture2D TileSet => TileMap?.Tilesets?.Values?.FirstOrDefault()?.Texture;
        public OverworldScreen OverworldScreen { get; }
        public Npc CropVendor { get; set; } //change into list later if needed

        public Town(OverworldScreen overworldScreen)
        {
            OverworldScreen = overworldScreen;
            PlayerSpawnPoint = new Vector2(240,356);
            PasableTileIds =
            [
                2994, // grass
                 //pavement
                1066, 1067,1068,1069,1070,1071,1072,1073,1074,1075,1076,1077,1078, 1079, 1080,1081,1082,1083,1084,
                1114,1115,0000,0000,1118,1119,1120,1121,0000,0000,0000,1125,1126,1127,1128,1129,0000,0000,1132,
                1162,1163,1164,1165,1166,1167,1168,1169,0000,0000,0000,1173,1174,1175,1176,1177,1178,1179,1180,
                1210,1211,0000,1213,1214,1215,1216,1217,0000,0000,0000,1221,1222,1223,1224,1225,0000,1227,1228,
                1258,1259,0000,1261,1262,1263,1264,1265,1266,1267,1268,1269,1270,1271,1272,1273,0000,1275,1276,
                1306,1307,1308,1309,1310,1311, 1312, 1313,1314,1315,1316,1317,1318,1319,1320,1321,1322,1323,1324,
                1354, 1355,1356,1357,1358,1359,1360,1361,1362,1363,1364,1365,1366,1367,1368,1369,1370,1371,1372,
                1402, 1403,1404,1405,1406,1407,1408,1409, 1413, 1414,1415,1416,1417,1418, 1419,1420,
                1445, 1446, 1447, 1448, 1449, 1450,1451,1452,1453,1454,1455,1456,1457,0000,0000,0000,0000, 1461,1462,1463,1464,1465,1466,1467,1468,1469,1470,1471,1472,
                1493, 1494, 1495, 1496, 1497, 1498, 1499, 1500, 1501, 1502, 1503, 1504, 1505,0000,0000,1509,1510,1511,1512,1513,1514,1515,1516,1517,1518,1519,1520,
                1545, 1546, 1547, 1548, 1549, 1550, 1551, 1552, 1553, 1554,1555, 1556, 1557, 1558, 1559, 1560, 1561, 1562, 1563, 1564, 1565,
                1594, 1595,1596,1597,1598,1599,1600,1601,1602,1603,1604,1605,1606,1607,1608,1609,1610,1611,1612,
                1642,1643,1644,1645,1646,1647,1648,1649,1650,1651,1652,1653,1654,1655,1656,1657,1658,1659,1660,
                1690,1691,0000,1693,1694,1695,1696,1697,1698,1699,1700,1701,1702,1703,1704,1705,0000,1707,1708,
                1738,1739,0000,1741,1742,1743,1744,1745,1746,1747,1748,1749,1750,1751,1752,1753,0000,1755,1756,
                1786,1787,1788,1789,1790,1791,1792,1793,1794,1795,1796,1797,1798,1799,1800,1801,1802,1803,1804,
                1834,1835,1836,1837,1838,1839,1840,1841,1842,1843,1844,1845,1846,1847,1848,1849,1850,1851,1852,
                1882,0000,0000,1885,0000,0000,1888,1889,1890,1891,1892,1893,0000,0000,1896,0000,0000,1899,1900,
                1937,1938,1939,1940,
                1985,1986,1987,1988,
                // pavement
                2165,2213, // path
                757,758, 759, 710,711, 805,806,807, // path to house
                
                488,489,490,491,492 ,440,441,442,443, 444,
                536,537,538,539, 580, 581,582,583,584,585,586,587, 632,633,634, 635 // city hall path
            ];

            var name = overworldScreen.Player.Name;

            CropVendor = new NpcBuilder<Vendor>()
                .SetName("Wilhelm the Market Master")
                .SetGreetings([
                    "Welcome to my market stall! We have the finest crops in the land!",
                    "Best prices for quality goods",
                    "I always look forward to seeing you, " + name + "!"
                ])
                .SetBeforeTradeDialogues([
                "My favorite supplier is back! What have you got today, " + name + "?",
                "I hope you brought something good to trade, " + name + "!",
              
                ])
                .SetAfterTradeDialogues([
                "Doing business with you is a real pleasure," + name + "!", 
                "It's always a pleasure doing business with you, "+ name +"!",
                "Quality goods as always, " + name +" Thank you!" +
                "My buyers will be thrilled with this quality, " + name + "!"
                ])
                .SetFarewells([
                    "Come back soon, friend!",
                    "Don't forget to tell your friends about us!"
                ])
                .SetPosition(new Vector2(735,745))
                .SetInteractionBounds(new Rectangle(735,780,32,32))
                .SetZoom(2.0f)
                .AddTradeItem(Crop.CreateCrop("carrot"))
                .Build();

        }
        
        private void SetupCropVendorInventory()
        {
            // Ensure inventory list is properly sized
            while (CropVendor.Inventory.Count < 30)
            {
                CropVendor.Inventory.Add(null);
            }
    
            try
            {
                // Add some crops (that NPC might buy from player)
                var carrotCrop = Crop.CreateCrop("carrot");
                carrotCrop.Amount = 5;
                CropVendor.Inventory[1] = carrotCrop;
        
                Console.WriteLine($"Setup {CropVendor.Name} inventory with {CropVendor.Inventory.Count(i => i != null)} items");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error setting up CropVendor inventory: {ex.Message}");
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
            
            CropVendor.Draw(spriteBatch); //change to list later
        }

        public void LoadContent(ContentManager content)
        {
            try
            {
                TileMap = Map.Load(content.RootDirectory + PathToTileMap, content);
            }
            catch (System.Exception ex)
            {
                System.Console.WriteLine($"Failed to load Town map: {ex.Message} although it is located in Content/Tilemaps xD");
                TileMap = null;
            }
            
            CropVendor?.LoadContent(content, @"Spritesheets\\crop_vendor"); //change to traversing list later
        }
    }
}