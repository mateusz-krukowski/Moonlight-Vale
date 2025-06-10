using Moonlight_Vale.Screens.Maps;

namespace Moonlight_Vale.Systems
{
    public class PlantGrowthSystem
    {
        public static PlantGrowthSystem Instance { get; } = new PlantGrowthSystem();
        public PlayerFarm PlayerFarm { get; private set; }
        private PlantGrowthSystem()
        {
        
        }

        public void CheckPlantsGrowthStage()
        {
        
        }
    }
}