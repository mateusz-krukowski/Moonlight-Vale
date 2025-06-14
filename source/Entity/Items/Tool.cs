namespace Moonlight_Vale.Entity.Items
{
    // Concrete Tool class since Item is abstract
    public class Tool : Item
    {
        public int Durability { get; private set; }
        public int MaxDurability { get; private set; }
        public enum ToolType { Shovel, Hoe, WateringCan, Scythe }
        public ToolType TypeOfTool { get;  set; }    

        public Tool(string name, string description, string iconPath, int stackSize, int price, int durability)
            : base(name, description, iconPath, stackSize, price)
        {
            Type = ItemType.Tool;
            Durability = durability;
            MaxDurability = durability;
        }
    }
    
}