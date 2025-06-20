namespace Moonlight_Vale.Entity.Items;

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

public static class PlantData
{
    private static Dictionary<string, PlantEntry> _plants = new();

    public static IReadOnlyDictionary<string, PlantEntry> All => _plants;

    public static void LoadFromJson(string path)
    {
        string json = File.ReadAllText(path);
        var plants = JsonSerializer.Deserialize<List<PlantEntry>>(json);

        _plants = plants.ToDictionary(
            p => p.name.ToLowerInvariant(),
            p => p
        );
    }

    public static PlantEntry Get(string name)
    {
        _plants.TryGetValue(name.ToLowerInvariant(), out var plant);
        return plant;
    }
}
public class PlantEntry
{
    public int id { get; set; }
    public string name { get; set; }
    public int farmingLevelRequired { get; set; }
    public int daysToGrow { get; set; }
    public string pathToSeedIcon { get; set; }
    public string pathToCropIcon { get; set; }
    public int seedPrice { get; set; }
    public int cropPrice { get; set; }
    public string description { get; set; }
}