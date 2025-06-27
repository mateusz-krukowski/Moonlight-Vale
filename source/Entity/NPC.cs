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
        private const int DEFAULT_INVENTORY_SIZE = 30;
        
        protected Texture2D spriteSheet;
        protected Texture2D sprite;
        protected ContentManager _contentManager;
        

        // Dialogue system - dictionary containing lists of dialogue options
        protected Dictionary<string, List<string>> dialogues;
        protected Random random;

        public Vector2 Position { get; set; }
        public float Zoom { get; set; } = 2.0f;

        public List<Item> Inventory { get; set; } = new List<Item>(DEFAULT_INVENTORY_SIZE);
        public string Name { get; set; } = "Unnamed NPC";
        public Rectangle InteractionBounds { get; set; } = Rectangle.Empty;

        // Make constructor internal so only builder can create instances
        internal Npc()
        {
            // Initialize dialogue system
            dialogues = new Dictionary<string, List<string>>();
            random = new Random();

            // Initialize empty dialogue categories
            InitializeDialogueCategories();
            
            // Initialize inventory with proper size and null safety
            InitializeInventory();
        }

        /// <summary>
        /// Initialize inventory with default size and null safety (like Player)
        /// </summary>
        private void InitializeInventory()
        {
            // Ensure inventory is properly sized with nulls
            while (Inventory.Count < DEFAULT_INVENTORY_SIZE)
            {
                Inventory.Add(null);
            }
        }

        /// <summary>
        /// Ensure inventory has at least the specified size (like Player does)
        /// </summary>
        private void EnsureInventorySize(int requiredSize)
        {
            while (Inventory.Count <= requiredSize)
            {
                Inventory.Add(null);
            }
        }

        /// <summary>
        /// Find first empty inventory slot (like Player)
        /// </summary>
        public int FindFirstEmptyInventorySlot()
        {
            for (int i = 0; i < DEFAULT_INVENTORY_SIZE; i++)
            {
                if (i >= Inventory.Count || Inventory[i] == null)
                {
                    return i;
                }
            }
            return -1;
        }

        /// <summary>
        /// Add item to inventory with proper null safety and stacking (like Player)
        /// </summary>
        public void AddItemToInventory(Item item, int amount = 1)
        {
            if (item == null) return;
            
            // First try to stack with existing items
            for (int i = 0; i < Inventory.Count; i++)
            {
                var inventoryItem = Inventory[i];
                if (inventoryItem != null && inventoryItem.Name == item.Name && 
                    inventoryItem.Amount < inventoryItem.StackSize)
                {
                    int spaceAvailable = inventoryItem.StackSize - inventoryItem.Amount;
                    int amountToAdd = Math.Min(amount, spaceAvailable);
                    
                    inventoryItem.Amount += amountToAdd;
                    amount -= amountToAdd;
                    
                    Console.WriteLine($"NPC {Name}: Stacked {amountToAdd}x {item.Name} with inventory slot {i}.");
                    
                    if (amount <= 0) return; // All items stacked successfully
                }
            }

            // Find first empty slot for remaining items
            while (amount > 0)
            {
                int emptySlot = FindFirstEmptyInventorySlot();
                if (emptySlot == -1)
                {
                    Console.WriteLine($"NPC {Name}: Inventory is full! Cannot add more items.");
                    return;
                }

                // Ensure inventory list is large enough (like Player does)
                EnsureInventorySize(emptySlot);

                // Create new item for this slot
                var newItem = CreateItemCopy(item);
                newItem.Amount = Math.Min(amount, item.StackSize);
                
                if (_contentManager != null)
                {
                    newItem.LoadContent(_contentManager);
                }

                Inventory[emptySlot] = newItem;
                amount -= newItem.Amount;
                
                Console.WriteLine($"NPC {Name}: Added {newItem.Amount}x {newItem.Name} to inventory slot {emptySlot}.");
            }
        }

        /// <summary>
        /// Swap items within NPC inventory (like Player.SwapInventoryItems)
        /// </summary>
        public void SwapInventoryItems(int index1, int index2)
        {
            // Ensure both indices are valid and inventory is large enough
            EnsureInventorySize(Math.Max(index1, index2));

            var temp = Inventory[index1];
            Inventory[index1] = Inventory[index2];
            Inventory[index2] = temp;

            Console.WriteLine($"NPC {Name}: Swapped inventory items: [{index1}] <-> [{index2}]");
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
    var random = new Random();
    
    // Generic trivia that all NPCs can say
    var genericTrivia = new[]
    {
        "Life in this town is peaceful. I love watching the seasons change.",
        "The weather has been quite nice lately, perfect for farming!",
        "I've lived here all my life. This place holds many memories for me.",
        "The community here is wonderful. Everyone helps each other out.",
        "There's something magical about watching crops grow from tiny seeds.",
        "The sunrise over the fields is always a beautiful sight to behold.",
        "I remember when this town was much smaller. Time really flies by.",
        "Nothing beats the smell of fresh soil after a good rain.",
        "The local festivals are always so much fun. Do you participate?",
        "Sometimes I just sit and watch the clouds roll by. Very peaceful."
    };
    
    // Specific trivia for different NPCs
    if (Name.Contains("Market Master") || Name.Contains("Wilhelm"))
    {
        var marketSpecificTrivia = new[]
        {
            "I've been running this market for over 20 years. Seen all kinds of crops come and go!",
            "Did you know that carrots were originally purple? Orange carrots are a relatively modern invention!",
            "The best time to plant tomatoes is right after the last frost. They're quite sensitive to cold.",
            "I once sold a pumpkin that weighed over 50 pounds! The farmer was so proud of it.",
            "Market prices fluctuate based on seasons. Smart farmers know when to plant and when to sell!",
            "The secret to good business is treating every customer like family.",
            "I can tell the quality of produce just by looking at it. Years of experience!",
            "Some farmers bring me the most unusual vegetables. Nature is full of surprises."
        };
        
        // Combine market-specific and generic trivia
        var allMarketTrivia = marketSpecificTrivia.Concat(genericTrivia).ToArray();
        return allMarketTrivia[random.Next(allMarketTrivia.Length)];
    }
    else if (Name.Contains("Shopkeeper"))
    {
        var shopSpecificTrivia = new[]
        {
            "This shop has been in my family for three generations. My grandfather started it with just a few tools.",
            "The secret to good farming tools is proper maintenance. A well-cared hoe can last decades!",
            "I import some of my best tools from the mountain regions. The blacksmiths there are legendary.",
            "Did you know that watering plants in the evening is better than morning? Less evaporation!",
            "Every season brings different challenges. That's why having good, reliable tools is essential.",
            "A good farmer is only as good as their tools. I make sure to stock the best quality.",
            "I've seen farming techniques change over the years. Innovation is important!",
            "The sound of a well-made tool working the soil is music to my ears."
        };
        
        // Combine shop-specific and generic trivia
        var allShopTrivia = shopSpecificTrivia.Concat(genericTrivia).ToArray();
        return allShopTrivia[random.Next(allShopTrivia.Length)];
    }
    else
    {
        // Other NPCs get only generic trivia
        return genericTrivia[random.Next(genericTrivia.Length)];
    }
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
            _contentManager = content; // Store the content manager for later use
            sprite = content.Load<Texture2D>(spritePath);
            
            // Load content for all inventory items (with null safety)
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

        public bool CanInteract(Player player)
        {
            if (InteractionBounds == Rectangle.Empty)
            {
                return false;

            }

            // Check if player's center position is within interaction bounds
            Vector2 playerCenter = new Vector2(
                player.Position.X + (player.SpriteWidth * player.Zoom / 2),
                player.Position.Y + (player.SpriteHeight * player.Zoom / 2)
            );

            if (InteractionBounds.Contains(playerCenter))
            {
                Console.WriteLine("interaction possible");
                return true;
            }

            return false;
        }

        /// <summary>
        /// Move item from NPC inventory to Player inventory (Player buying from NPC)
        /// Enhanced with Player-like safety mechanisms
        /// </summary>
        public bool MoveToPlayerInventory(int npcInventoryIndex, int playerInventoryIndex, Player player)
        {
            if (player == null)
            {
                Console.WriteLine("Player is null!");
                return false;
            }

            if (npcInventoryIndex < 0 || npcInventoryIndex >= Inventory.Count)
            {
                Console.WriteLine($"Invalid NPC inventory index: {npcInventoryIndex}");
                return false;
            }

            var npcItem = Inventory[npcInventoryIndex];
            if (npcItem == null)
            {
                Console.WriteLine("No item at NPC inventory index");
                return false;
            }

            int amountToSell = npcItem.Amount;
            if (amountToSell <= 0)
            {
                Console.WriteLine("Nothing to sell!");
                return false;
            }

            int totalPrice = npcItem.Price * amountToSell;
            
            if (player.Gold < totalPrice)
            {
                Console.WriteLine($"Player doesn't have enough gold! Need ${totalPrice}, has ${player.Gold}");
                return false;
            }
            
            while (player.Inventory.Count <= playerInventoryIndex)
            {
                player.Inventory.Add(null);
            }

            var existingPlayerItem = player.Inventory[playerInventoryIndex];
            
            player.Gold -= totalPrice;
            
            var itemCopyForPlayer = CreateItemCopy(npcItem);
            itemCopyForPlayer.Amount = amountToSell;

            if (_contentManager != null)
            {
                itemCopyForPlayer.LoadContent(_contentManager);
            }

            
            player.Inventory[playerInventoryIndex] = itemCopyForPlayer;
            
            Inventory[npcInventoryIndex] = existingPlayerItem;

            Console.WriteLine($"Player bought {itemCopyForPlayer.Name} x{amountToSell} for ${totalPrice}");
            return true;
        }


        /// <summary>
        /// Move item from Player inventory to NPC inventory (Player selling to NPC)
        /// Enhanced with Player-like safety mechanisms
        /// </summary>
        public bool MoveFromPlayerInventory(int playerInventoryIndex, int npcInventoryIndex, Player player)
        {
            if (player == null)
            {
                Console.WriteLine("Player is null!");
                return false;
            }

            if (playerInventoryIndex < 0 || playerInventoryIndex >= player.Inventory.Count)
            {
                Console.WriteLine($"Invalid player inventory index: {playerInventoryIndex}");
                return false;
            }

            var playerItem = player.Inventory[playerInventoryIndex];
            if (playerItem == null)
            {
                Console.WriteLine("No item at player inventory index");
                return false;
            }

            // Ensure NPC inventory is large enough (like Player does)
            EnsureInventorySize(npcInventoryIndex);

            var existingNpcItem = Inventory[npcInventoryIndex];
            
            int amountToSell = playerItem.Amount;

            if (amountToSell <= 0)
            {
                Console.WriteLine("Nothing to sell!");
                return false;
            }

            
            int sellPrice = (int)(playerItem.Price * 0.7f) * amountToSell;
            
            player.Gold += sellPrice;
            
            var itemCopyForNpc = CreateItemCopy(playerItem);
            itemCopyForNpc.Amount = amountToSell;

            if (_contentManager != null)
            {
                itemCopyForNpc.LoadContent(_contentManager);
            }
            
            Inventory[npcInventoryIndex] = itemCopyForNpc;
            
            player.Inventory[playerInventoryIndex] = existingNpcItem;

            Console.WriteLine($"Player sold {itemCopyForNpc.Name} x{amountToSell} for ${sellPrice}");
            return true;
        }


        /// <summary>
        /// Swap items between NPC and Player inventories (direct exchange)
        /// Enhanced with Player-like safety mechanisms
        /// </summary>
        public void SwapWithPlayerInventory(int npcInventoryIndex, int playerInventoryIndex, Player player)
        {
            if (player == null)
            {
                Console.WriteLine("Player is null!");
                return;
            }

            // Ensure both inventories are large enough (like Player does)
            EnsureInventorySize(npcInventoryIndex);
            while (player.Inventory.Count <= playerInventoryIndex)
            {
                player.Inventory.Add(null);
            }

            // Simple swap without money exchange
            var tempItem = Inventory[npcInventoryIndex];
            Inventory[npcInventoryIndex] = player.Inventory[playerInventoryIndex];
            player.Inventory[playerInventoryIndex] = tempItem;
            
            Console.WriteLine($"Swapped NPC[{npcInventoryIndex}] with Player[{playerInventoryIndex}]");
        }

        /// <summary>
        /// Try to stack items like Player.TryStackItems
        /// </summary>
        public bool TryStackItems(Item sourceItem, Item targetItem)
        {
            if (sourceItem == null || targetItem == null)
                return false;

            if (sourceItem.GetType() == targetItem.GetType() && 
                sourceItem.Name == targetItem.Name && 
                targetItem.Amount < targetItem.StackSize)
            {
                int spaceAvailable = targetItem.StackSize - targetItem.Amount;
                int amountToTransfer = Math.Min(sourceItem.Amount, spaceAvailable);

                targetItem.Amount += amountToTransfer;
                sourceItem.Amount -= amountToTransfer;

                return sourceItem.Amount == 0;
            }

            return false;
        }

        /// <summary>
        /// Helper method to create a copy of an item (with enhanced error handling)
        /// </summary>
        private Item CreateItemCopy(Item original)
        {
            if (original == null)
            {
                throw new ArgumentNullException(nameof(original), "Cannot create copy of null item");
            }

            try
            {
                if (original is Seed seed)
                {
                    string plantName = original.Name.Replace(" seed", "");
                    return Seed.CreateSeed(plantName);
                }
                else if (original is Crop crop)
                {
                    return Crop.CreateCrop(original.Name);
                }
                else if (original is Tool tool)
                {
                    return new Tool(tool.Name, tool.Description, tool.IconPath, 
                                   tool.StackSize, tool.Price, tool.Durability, tool.TypeOfTool);
                }
                
                throw new NotImplementedException($"Cannot create copy of item type: {original.GetType()}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating item copy for {original.Name}: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Print inventory contents for debugging (like Player.PrintInventory)
        /// </summary>
        public void PrintInventory()
        {
            Console.WriteLine($"=== {Name.ToUpper()} INVENTORY ===");
            Console.WriteLine($"Items: {Inventory.Count(i => i != null)}/{DEFAULT_INVENTORY_SIZE}");
            Console.WriteLine("------------------------");
    
            if (Inventory.Count(i => i != null) == 0)
            {
                Console.WriteLine("Inventory is empty!");
                return;
            }
    
            for (int i = 0; i < Inventory.Count; i++)
            {
                var item = Inventory[i];
                if (item != null)
                {
                    Console.WriteLine($"[{i:D2}] {item.Name} x {item.Amount} (${item.Price})");
                }
                else
                {
                    Console.WriteLine($"[{i:D2}] NULL");
                }
            }
            Console.WriteLine("========================");
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
            if (item != null)
            {
                tradeItems.Add(item);
            }
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
            return tradeItems.Any(item => item?.Name == itemName);
        }

        public Item GetItem(string itemName)
        {
            return tradeItems.FirstOrDefault(item => item?.Name == itemName);
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
                vendor.AddItemToInventory(item);
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
        
        public NpcBuilder<T> SetInteractionBounds(Rectangle bounds)
        {
            npc.InteractionBounds = bounds;
            return this;
        }

        public NpcBuilder<T> SetInteractionBounds(int x, int y, int width, int height)
        {
            npc.InteractionBounds = new Rectangle(x, y, width, height);
            return this;
        }

        public T Build()
        {
            return npc;
        }
    }
}