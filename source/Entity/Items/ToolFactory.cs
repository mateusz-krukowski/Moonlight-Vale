using System;

namespace Moonlight_Vale.Entity.Items
{
    /// <summary>
    /// Factory class for creating various tools.
    /// </summary>
    /// <remarks>
    /// This class provides methods to create different types of tools with predefined properties.
    /// </remarks>
    public static class ToolFactory
    {
        private static Tool CreateShovel()
        {
            var tool = new Tool(
                name: "Shovel",
                description: "Used for digging and tilling soil",
                iconPath: @"Icons\Tools\shovel.png",
                stackSize: 1,
                price: 100,
                durability: 50
            )
            {
                TypeOfTool = Tool.ToolType.Shovel
            };
            return tool;
        }

        private static Tool CreateHoe()
        {
            var tool = new Tool(
                name: "Hoe",
                description: "Used for preparing soil for planting",
                iconPath: @"Icons\Tools\hoe.png",
                stackSize: 1,
                price: 80,
                durability: 60
            )
            {
                TypeOfTool = Tool.ToolType.Hoe
            };
            return tool;
        }

        private static Tool CreateScythe()
        {
            var tool = new Tool(
                name: "Scythe",
                description: "Used for harvesting crops and cutting grass",
                iconPath: @"Icons\Tools\scythe.png",
                stackSize: 1,
                price: 120,
                durability: 45
            )
            {
                TypeOfTool = Tool.ToolType.Scythe
            };
            return tool;
        }

        private static Tool CreateWateringCan()
        {
            var tool = new Tool(
                name: "Watering Can",
                description: "Used for watering plants",
                iconPath: @"Icons\Tools\watering_can.png",
                stackSize: 1,
                price: 90,
                durability: 100
            )
            {
                TypeOfTool = Tool.ToolType.WateringCan
            };
            return tool;
        }

        public static Tool CreateTool(Tool.ToolType toolType)
        {
            return toolType switch
            {
                Tool.ToolType.Shovel => CreateShovel(),
                Tool.ToolType.Hoe => CreateHoe(),
                Tool.ToolType.Scythe => CreateScythe(),
                Tool.ToolType.WateringCan => CreateWateringCan(),
                _ => throw new ArgumentException($"Unknown tool type: {toolType}")
            };
        }
    }
}