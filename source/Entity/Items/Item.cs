using System;
using Microsoft.Xna.Framework.Graphics;

namespace Moonlight_Vale.Entity.Items
{
    public abstract class Item
    {
        public String Name { get; private set; }
        public String IconPath { get; private set; }
        public String Description { get; private set; }
        public Texture2D Icon { get; private set; }

        public int StackSize { get; private set; }
        public int Price { get; private set; }
        public enum ItemType { Tool, Seed, Crop, Food, Miscellaneous }
        public ItemType Type { get; protected set; }    


        public Item(String name,  String description, String iconPath, int stackSize, int price)
        {
            Name = name;
            Description = description;
            StackSize = stackSize;
            Price = price;
            IconPath = iconPath;
        }
        public virtual void LoadContent(GraphicsDevice graphicsDevice)
        {
            if (Icon == null && !String.IsNullOrEmpty(IconPath))
            {
                Icon = Texture2D.FromFile(graphicsDevice, IconPath);
            }
        }
    }
}