using System;

namespace Moonlight_Vale.Entity.Items;

public class Seed : Plant
{
    
    private Seed(string name, string description, string iconPath, int stackSize, int price) : base(name, description, iconPath, stackSize, price)
    {
    }
    
    public static Seed CreateSeed(string name)
    {
        var plant = PlantData.Get(name);

        if (plant == null)
        {
            // Return a default seed or throw meaningful exception
            throw new InvalidOperationException(
                $"Plant data for '{name}' not found. Make sure PlantData.LoadFromJson() is called first.");
        }

        return new Seed(
            name:name +" seed",
            description:$"This will grow into a {name}",
            iconPath:plant.pathToSeedIcon,
            price:plant.seedPrice,
            stackSize: 64);
        }
}
