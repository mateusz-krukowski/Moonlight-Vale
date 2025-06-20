using System;
using Microsoft.Xna.Framework;
using Moonlight_Vale.Entity.Items;

namespace Moonlight_Vale.Entity;

public class GrowingPlant
{
    public bool IsReadyToHarvest { get; set; } = false;
    public bool isWatered { get; set; } = false;
    public int PlantingDay { get; set; }
    public int DaysToGrow { get; }
    public string Name { get; }
    public Vector2 Position { get; set; } = Vector2.Zero;
    
    // Track the initial tile type when planted (111 for dry, 112 for watered)
    public int InitialTileType { get; set; }
    
    // Track current growth stage to avoid unnecessary tile updates
    public PlantGrowthStage CurrentStage { get; set; } = PlantGrowthStage.Seedling;

    public GrowingPlant(string name)
    {
        Name = name;
        
        var plant = PlantData.Get(name);
        if (plant == null)
        { 
            throw new InvalidOperationException($"Plant data for '{name}' in GrowingPlant not found. Make sure PlantData.LoadFromJson() is called first.");
        }
        DaysToGrow = plant.daysToGrow;
    }
    
    // Calculate days remaining to grow based on current game day
    public int GetDaysRemainingToGrow()
    {
        int currentDay = Moonlight_Vale.Systems.TimeSystem.Instance.CurrentDay;
        int daysGrown = currentDay - PlantingDay;
        return Math.Max(0, DaysToGrow - daysGrown);
    }
    
    // Get what the current growth stage should be
    public PlantGrowthStage GetCurrentGrowthStage()
    {
        int daysRemaining = GetDaysRemainingToGrow();
        
        if (daysRemaining == 0)
            return PlantGrowthStage.ReadyToHarvest;
        else if (daysRemaining <= DaysToGrow / 2) // Plant starts growing when halfway through growth period
            return PlantGrowthStage.Growing;
        else
            return PlantGrowthStage.Seedling;
    }
}

public enum PlantGrowthStage
{
    Seedling,    // Just planted (111 or 112)
    Growing,     // Partially grown (79 or 80)
    ReadyToHarvest // Fully grown (95)
}