using System;

namespace Moonlight_Vale.Entity.Items;

public class Crop : Plant
{
    public Crop(string name, string description, string iconPath, int stackSize, int price) : base(name, description, iconPath, stackSize, price)
    {
    }
    

    public static Crop CreateCrop(string name)
    {
        var plant = PlantData.Get(name);
        if (plant == null)
        {
            // Return a default crop or throw meaningful exception
            throw new InvalidOperationException(
                $"Plant data for '{name}' not found. Make sure PlantData.LoadFromJson() is called first.");
        }
        
        return new Crop(
            name: name,
            description:plant.description,
            iconPath: plant.pathToCropIcon,
            price: plant.cropPrice,
            stackSize: 64);

    }
}