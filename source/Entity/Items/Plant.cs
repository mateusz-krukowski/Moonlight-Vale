using System;

namespace Moonlight_Vale.Entity.Items;

public abstract class Plant : Item
{
    int daysToGrow;
    public bool ShouldBeRemoved { get; set; } = false;
    
    protected Plant(string name, string description, string iconPath, int stackSize, int price) : base(name, description, iconPath, stackSize, price)
    {
    }
}