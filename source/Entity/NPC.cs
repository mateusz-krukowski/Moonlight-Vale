using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using Moonlight_Vale.Entity.Items;

namespace Moonlight_Vale.Entity
{
    public class Npc
    {
        private const int SPRITE_WIDTH = 16;
        private const int SPRITE_HEIGHT = 24;
        protected Texture2D spriteSheet;
        protected Texture2D sprite;
        protected ContentManager _contentManager;
        

        // Dialogue system - dictionary containing lists of dialogue options
        protected Dictionary<string, List<string>> dialogues;
        protected Random random;

        public Vector2 Position { get; set; }
        public float Zoom { get; set; } = 2.0f;

        public List<Item> Inventory { get; set; } = new List<Item>(30);
        public string Name { get; set; } = "Unnamed NPC";

        // Make constructor internal so only builder can create instances
        internal Npc()
        {
            // Initialize dialogue system
            dialogues = new Dictionary<string, List<string>>();
            random = new Random();

            // Initialize empty dialogue categories
            InitializeDialogueCategories();
        }

        protected virtual void InitializeDialogueCategories()
        {
            dialogues["greetings"] = new List<string>();
            dialogues["farewells"] = new List<string>();
            dialogues["trivia"] = new List<string>();
        }

        public virtual void SetGreetings(List<string> greetingMessages)
        {
            dialogues["greetings"] = new List<string>(greetingMessages);
        }

        public virtual void SetFarewells(List<string> farewellMessages)
        {
            dialogues["farewells"] = new List<string>(farewellMessages);
        }

        public virtual void SetTrivia(List<string> triviaMessages)
        {
            dialogues["trivia"] = new List<string>(triviaMessages);
        }

        public string GetRandomGreeting()
        {
            return GetRandomDialogue("greetings");
        }

        public string GetRandomFarewell()
        {
            return GetRandomDialogue("farewells");
        }

        public string GetRandomTrivia()
        {
            return GetRandomDialogue("trivia");
        }

        protected string GetRandomDialogue(string category)
        {
            if (dialogues.ContainsKey(category) && dialogues[category].Any())
            {
                int randomIndex = random.Next(dialogues[category].Count);
                return dialogues[category][randomIndex];
            }

            return string.Empty;
        }

        public void AddGreeting(string greeting)
        {
            dialogues["greetings"].Add(greeting);
        }

        public void AddFarewell(string farewell)
        {
            dialogues["farewells"].Add(farewell);
        }

        public void AddTrivia(string trivia)
        {
            dialogues["trivia"].Add(trivia);
        }

        public int GetDialogueCount(string category)
        {
            if (dialogues.ContainsKey(category))
            {
                return dialogues[category].Count;
            }

            return 0;
        }

        public void LoadContent(ContentManager content, string spritePath)
        {
            _contentManager = content;
            sprite = content.Load<Texture2D>(spritePath);
            foreach (var item in Inventory)
            {
                item?.LoadContent(content);
            }
        }
        
        public void Draw(SpriteBatch spriteBatch)
        {
            if (sprite == null) return;
            
            spriteBatch.Draw(sprite, Position, null, Color.White, 0f, Vector2.Zero, Zoom, SpriteEffects.None, 0f);
        }
    }


    public class Vendor : Npc
    {
        private List<Item> tradeItems;

        public Vendor() : base()
        {
            tradeItems = new List<Item>();
        }

        protected override void InitializeDialogueCategories()
        {
            base.InitializeDialogueCategories();
            // Add vendor-specific dialogue categories
            dialogues["beforeTrade"] = new List<string>();
            dialogues["afterTrade"] = new List<string>();
        }

        public void SetTradeItems(List<Item> items)
        {
            tradeItems = new List<Item>(items);
        }

        public void AddTradeItem(Item item)
        {
            tradeItems.Add(item);
        }

        public void SetBeforeTradeDialogues(List<string> beforeTradeMessages)
        {
            dialogues["beforeTrade"] = new List<string>(beforeTradeMessages);
        }

        public void SetAfterTradeDialogues(List<string> afterTradeMessages)
        {
            dialogues["afterTrade"] = new List<string>(afterTradeMessages);
        }

        public string GetRandomBeforeTradeDialogue()
        {
            return GetRandomDialogue("beforeTrade");
        }

        public string GetRandomAfterTradeDialogue()
        {
            return GetRandomDialogue("afterTrade");
        }

        public List<Item> GetTradeItems()
        {
            return new List<Item>(tradeItems);
        }

        public bool HasItem(string itemName)
        {
            return tradeItems.Any(item => item.Name == itemName);
        }

        public Item GetItem(string itemName)
        {
            return tradeItems.FirstOrDefault(item => item.Name == itemName);
        }
    }

    public class NpcBuilder<T> where T : Npc, new()
    {
        private T npc;

        public NpcBuilder()
        {
            npc = (T)Activator.CreateInstance(typeof(T), true);
        }

        public NpcBuilder<T> SetName(string name)
        {
            npc.Name = name;
            return this;
        }

        public NpcBuilder<T> SetPosition(Vector2 position)
        {
            npc.Position = position;
            return this;
        }

        public NpcBuilder<T> SetZoom(float zoom)
        {
            npc.Zoom = zoom;
            return this;
        }

        public NpcBuilder<T> SetGreetings(List<string> greetings)
        {
            npc.SetGreetings(greetings);
            return this;
        }

        public NpcBuilder<T> SetFarewells(List<string> farewells)
        {
            npc.SetFarewells(farewells);
            return this;
        }

        public NpcBuilder<T> SetTrivia(List<string> trivia)
        {
            npc.SetTrivia(trivia);
            return this;
        }

        public NpcBuilder<T> AddGreeting(string greeting)
        {
            npc.AddGreeting(greeting);
            return this;
        }

        public NpcBuilder<T> AddFarewell(string farewell)
        {
            npc.AddFarewell(farewell);
            return this;
        }

        public NpcBuilder<T> AddTrivia(string trivia)
        {
            npc.AddTrivia(trivia);
            return this;
        }

        // Vendor-specific methods - only available when T is Vendor
        public NpcBuilder<T> SetTradeItems(List<Item> items)
        {
            if (npc is Vendor vendor)
            {
                vendor.SetTradeItems(items);
                return this;
            }

            throw new InvalidOperationException("SetTradeItems can only be used with Vendor NPCs");
        }

        public NpcBuilder<T> AddTradeItem(Item item)
        {
            if (npc is Vendor vendor)
            {
                vendor.AddTradeItem(item);
                return this;
            }

            throw new InvalidOperationException("AddTradeItem can only be used with Vendor NPCs");
        }

        public NpcBuilder<T> SetBeforeTradeDialogues(List<string> beforeTradeDialogues)
        {
            if (npc is Vendor vendor)
            {
                vendor.SetBeforeTradeDialogues(beforeTradeDialogues);
                return this;
            }

            throw new InvalidOperationException("SetBeforeTradeDialogues can only be used with Vendor NPCs");
        }

        public NpcBuilder<T> SetAfterTradeDialogues(List<string> afterTradeDialogues)
        {
            if (npc is Vendor vendor)
            {
                vendor.SetAfterTradeDialogues(afterTradeDialogues);
                return this;
            }

            throw new InvalidOperationException("SetAfterTradeDialogues can only be used with Vendor NPCs");
        }

        public T Build()
        {
            return npc;
        }
    }
}