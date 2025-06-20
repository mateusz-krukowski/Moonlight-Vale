using System.Collections.Generic;
using Moonlight_Vale.Entity;
using Moonlight_Vale.Screens.Maps;
using System.Linq;

namespace Moonlight_Vale.Systems
{
    public class PlantGrowthSystem
    {
        public static PlantGrowthSystem Instance { get; } = new PlantGrowthSystem();
        public List<GrowingPlant> plantsOnFarm;
        
        private PlantGrowthSystem()
        {
            plantsOnFarm = new List<GrowingPlant>();
        }

        public void CheckPlantsGrowthStage()
        {
            // Iterate through all plants and check their growth status
            for (int i = plantsOnFarm.Count - 1; i >= 0; i--)
            {
                var plant = plantsOnFarm[i];
                var expectedStage = plant.GetCurrentGrowthStage();
                
                // If plant stage has changed, update the tile
                if (plant.CurrentStage != expectedStage)
                {
                    UpdatePlantTileInSave(plant, expectedStage);
                    plant.CurrentStage = expectedStage;
                    
                    // Mark as ready to harvest if fully grown
                    if (expectedStage == PlantGrowthStage.ReadyToHarvest)
                    {
                        plant.IsReadyToHarvest = true;
                    }
                }
            }
        }
        
        private void UpdatePlantTileInSave(GrowingPlant plant, PlantGrowthStage newStage)
        {
            int newTileId = GetTileIdForStage(plant, newStage);
            
            // Use SavingSystem to modify the tile in the saved map file
            // Farm map width is always 44 tiles
            int mapWidth = 44;
            
            SavingSystem.Instance.ModifyTileInSavedMap(
                (int)plant.Position.X, 
                (int)plant.Position.Y, 
                newTileId, 
                mapWidth
            );
        }
        
        private int GetTileIdForStage(GrowingPlant plant, PlantGrowthStage stage)
        {
            return stage switch
            {
                PlantGrowthStage.Seedling => plant.InitialTileType, // 111 or 112
                PlantGrowthStage.Growing => plant.InitialTileType == 112 ? 80 : 79, // 80 if watered, 79 if dry
                PlantGrowthStage.ReadyToHarvest => 95, // Always 95 when ready to harvest
                _ => plant.InitialTileType
            };
        }
        
        // Add a plant to the farm at specific coordinates
        public void PlantSeed(string seedName, int tileX, int tileY, int initialTileType)
        {
            // Check if there's already a plant at this position
            var existingPlant = plantsOnFarm.Find(p => p.Position.X == tileX && p.Position.Y == tileY);
            if (existingPlant != null)
            {
                // Remove existing plant before planting new one
                plantsOnFarm.Remove(existingPlant);
            }
            
            // Create new growing plant
            var newPlant = new GrowingPlant(seedName);
            newPlant.Position = new Microsoft.Xna.Framework.Vector2(tileX, tileY);
            newPlant.PlantingDay = TimeSystem.Instance.CurrentDay;
            newPlant.InitialTileType = initialTileType; // Store whether it was 111 or 112
            newPlant.isWatered = initialTileType == 112; // Set watered status
            
            // Add to the plants list
            plantsOnFarm.Add(newPlant);
        }
        
        // Remove a plant from specific coordinates
        public void RemovePlant(int tileX, int tileY)
        {
            var plantToRemove = plantsOnFarm.Find(p => p.Position.X == tileX && p.Position.Y == tileY);
            if (plantToRemove != null)
            {
                plantsOnFarm.Remove(plantToRemove);
            }
        }
        
        // Get plant at specific coordinates
        public GrowingPlant GetPlantAt(int tileX, int tileY)
        {
            return plantsOnFarm.Find(p => p.Position.X == tileX && p.Position.Y == tileY);
        }
    }
}