using System.Collections.Generic;

namespace Moonlight_Vale.Entity.Items
{
    public class Tool : Item
    {
        public int Durability { get; private set; }
        public int MaxDurability { get; private set; }
        public enum ToolType { Shovel, Hoe, WateringCan, Scythe }
        public ToolType TypeOfTool { get; set; }    

        public Tool(string name, string description, string iconPath, int stackSize, int price, int durability, ToolType toolType)
            : base(name, description, iconPath, stackSize, price)
        {
            Type = ItemType.Tool;
            Durability = durability;
            MaxDurability = durability;
            TypeOfTool = toolType;
        }

        // Static method to create all basic tools at once
        public static List<Tool> CreateBasicToolset()
        {
            var tools = new List<Tool>
            {
                new Tool(
                    name: "Shovel",
                    description: "Used for digging and tilling soil",
                    iconPath: @"Icons\Tools\shovel",
                    stackSize: 1,
                    price: 100,
                    durability: 50,
                    toolType: ToolType.Shovel
                ),
                new Tool(
                    name: "Hoe",
                    description: "Used for preparing soil for planting",
                    iconPath: @"Icons\Tools\hoe",
                    stackSize: 1,
                    price: 80,
                    durability: 60,
                    toolType: ToolType.Hoe
                ),
                new Tool(
                    name: "Watering Can",
                    description: "Used for watering plants",
                    iconPath: @"Icons\Tools\watering_can",
                    stackSize: 1,
                    price: 90,
                    durability: 100,
                    toolType: ToolType.WateringCan
                ),
                new Tool(
                    name: "Scythe",
                    description: "Used for harvesting crops",
                    iconPath: @"Icons\Tools\scythe",
                    stackSize: 1,
                    price: 120,
                    durability: 45,
                    toolType: ToolType.Scythe
                )
            };

            return tools;
        }
    }
}