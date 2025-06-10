using System;
using Microsoft.Xna.Framework.Graphics;

namespace Moonlight_Vale.Entity.Items
{
    public abstract class Item
    {
        public String Name { get; private set; }
        public String Description { get; private set; }
        public Texture2D Icon { get; private set; }

        public int StackSize { get; private set; }
        public int Price { get; private set; }
        public Item(String name,  String description, Texture2D icon, int stackSize, int price)
        {
            Name = name;
            Icon = icon;
            Description = description;
            StackSize = stackSize;
            Price = price;
        }
    }
}