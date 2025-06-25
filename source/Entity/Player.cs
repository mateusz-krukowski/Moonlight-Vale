using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Moonlight_Vale.Entity.Items;
using Moonlight_Vale.Screens;
using Moonlight_Vale.Screens.Maps;
using Moonlight_Vale.Systems;
using System.Linq;

namespace Moonlight_Vale.Entity
{
    public class Player
    {
        private const int SPRITE_WIDTH = 16;
        private const int SPRITE_HEIGHT = 24;
        private const float HEAD_OFFSET = 12f;
        private const float DEFAULT_SPEED = 190f;
        private const float SPRINT_MULTIPLIER = 1.4f;
        private const float ANIMATION_SPEED = 0.15f;
        private const float TOOL_COOLDOWN = 0.1f;

        private float _energy = 100f;
        private float _toolCooldownTimer = 0f;

        private Texture2D spriteSheet;
        private ContentManager _contentManager;

        public SpriteEffects SpriteEffect { get; private set; } = SpriteEffects.None;
        public int Frame { get; private set; } = 1;
        public float AnimationTimer { get; private set; }
        public int CurrentRow { get; private set; }
        public bool Ascending { get; private set; } = true;

        public List<Item> Inventory { get; } = new List<Item>(30);
        public List<Item> ActionBar { get; } = new List<Item>(10);

        public OverworldScreen overworldScreen { get; set; }
        
        public string Name { get; private set; } = "Player";
        public Vector2 Position { get; set; }
        public Vector2 Velocity { get; set; }
        public Vector2 Direction { get; set; }
        public float Zoom { get; set; } = 2.0f;
        public float Speed { get; set; } = DEFAULT_SPEED;

        public float UpBorder { get; private set; }
        public float DownBorder { get; private set; }
        public float LeftBorder { get; private set; }
        public float RightBorder { get; private set; }

        public int SelectedItem { get; set; }

        public IMap Map { get; set; }

        public int SpriteWidth => SPRITE_WIDTH;
        public int SpriteHeight => SPRITE_HEIGHT;
        public float HeadOffset => HEAD_OFFSET;
        public float AnimationSpeed => ANIMATION_SPEED;

        public float Energy
        {
            get { return _energy; }
            set
            {
                if (value >= 0 && value <= 100)
                    _energy = value;
            }
        }

        public Player(Vector2 startPosition, IMap map, OverworldScreen overworldScreen)
        {
            if (overworldScreen.newGame == true)
            {
                Position = startPosition;
                InitializeBasicTools();
                InitializeSeeds();
            }
            else
            {
                
            }
            
            Map = map;
            this.overworldScreen = overworldScreen;
            UpdateBorders();
        }

        public void LoadContent(ContentManager content, string spriteSheetPath)
        {
            _contentManager = content;
            spriteSheet = content.Load<Texture2D>(spriteSheetPath);
            
            foreach (var item in ActionBar)
            {
                item?.LoadContent(content);
            }

            foreach (var item in Inventory)
            {
                item?.LoadContent(content);
            }
        }

        public void Update(GameTime gameTime, KeyboardState keyboard, MouseState mouse, MouseState previousMouseState)
        {
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            Velocity = Vector2.Zero;

            if (_toolCooldownTimer > 0)
                _toolCooldownTimer -= deltaTime;

            float currentSpeed = Speed;
            if (keyboard.IsKeyDown(Keys.LeftShift))
                currentSpeed *= SPRINT_MULTIPLIER;

            switch (keyboard)
            {
                case var k when k.IsKeyDown(Keys.W):
                    Move(Vector2.UnitY * -currentSpeed, deltaTime, 2);
                    break;
                case var k when k.IsKeyDown(Keys.S):
                    Move(Vector2.UnitY * currentSpeed, deltaTime, 0);
                    break;
                case var k when k.IsKeyDown(Keys.A):
                    Move(Vector2.UnitX * -currentSpeed, deltaTime, 1, SpriteEffects.None);
                    break;
                case var k when k.IsKeyDown(Keys.D):
                    Move(Vector2.UnitX * currentSpeed, deltaTime, 1, SpriteEffects.FlipHorizontally);
                    break;
                default:
                    Frame = 1;
                    break;
            }

            switch (keyboard)
            {
                case var k when k.IsKeyDown(Keys.D1): SelectedItem = 0; break;
                case var k when k.IsKeyDown(Keys.D2): SelectedItem = 1; break;
                case var k when k.IsKeyDown(Keys.D3): SelectedItem = 2; break;
                case var k when k.IsKeyDown(Keys.D4): SelectedItem = 3; break;
                case var k when k.IsKeyDown(Keys.D5): SelectedItem = 4; break;
                case var k when k.IsKeyDown(Keys.D6): SelectedItem = 5; break;
                case var k when k.IsKeyDown(Keys.D7): SelectedItem = 6; break;
                case var k when k.IsKeyDown(Keys.D8): SelectedItem = 7; break;
                case var k when k.IsKeyDown(Keys.D9): SelectedItem = 8; break;
                case var k when k.IsKeyDown(Keys.D0): SelectedItem = 9; break;
            }

            if (mouse.LeftButton == ButtonState.Pressed && 
                previousMouseState.LeftButton == ButtonState.Released && 
                _toolCooldownTimer <= 0)
            {
               if(!overworldScreen.isMouseOverlayingHUD) 
               {
                   UseToolOrSeed();
                   _toolCooldownTimer = TOOL_COOLDOWN;
               }
            }

            if (mouse.RightButton == ButtonState.Pressed && 
                previousMouseState.RightButton == ButtonState.Released && 
                _toolCooldownTimer <= 0)
            {
               if(!overworldScreen.isMouseOverlayingHUD && Map is PlayerFarm) 
               {
                   HarvestCrop();
                   _toolCooldownTimer = TOOL_COOLDOWN;
               }
            }
        }

        private void Move(Vector2 direction, float deltaTime, int row, SpriteEffects effect = SpriteEffects.None)
        {
            SpriteEffect = effect;
            CurrentRow = row;

            Direction = direction != Vector2.Zero ? Vector2.Normalize(direction) : Direction;

            Vector2 originalPosition = Position;
            Vector2 newPosition = originalPosition;
            float scaledHeadOffset = HeadOffset * Zoom;

            if (direction.X != 0)
            {
                float newX = originalPosition.X + direction.X * deltaTime;
                float tempLeft = newX;
                float tempRight = newX + (SpriteWidth * Zoom);
                float currentUp = originalPosition.Y + scaledHeadOffset;
                float currentDown = originalPosition.Y + (SpriteHeight * Zoom);

                bool canMove = direction.X < 0
                    ? CanMoveToTile(new Vector2(tempLeft, currentUp)) && CanMoveToTile(new Vector2(tempLeft, currentDown))
                    : CanMoveToTile(new Vector2(tempRight, currentUp)) && CanMoveToTile(new Vector2(tempRight, currentDown));

                if (canMove) newPosition.X = newX;
            }

            if (direction.Y != 0)
            {
                float newY = originalPosition.Y + direction.Y * deltaTime;
                float tempUp = newY + scaledHeadOffset;
                float tempDown = newY + (SpriteHeight * Zoom);
                float currentLeft = newPosition.X;
                float currentRight = newPosition.X + (SpriteWidth * Zoom);

                bool canMove = direction.Y < 0
                    ? CanMoveToTile(new Vector2(currentLeft, tempUp)) && CanMoveToTile(new Vector2(currentRight, tempUp))
                    : CanMoveToTile(new Vector2(currentLeft, tempDown)) && CanMoveToTile(new Vector2(currentRight, tempDown));

                if (canMove) newPosition.Y = newY;
            }

            Position = newPosition;
            UpdateAnimation(deltaTime);
            UpdateBorders();
        }

        private void UpdateBorders()
        {
            float scaledHeadOffset = HeadOffset * Zoom;
            UpBorder = Position.Y + scaledHeadOffset;
            DownBorder = Position.Y + (SpriteHeight * Zoom);
            LeftBorder = Position.X;
            RightBorder = Position.X + (SpriteWidth * Zoom);
        }

        private bool CanMoveToTile(Vector2 borderPosition)
        {
            if (Map?.TileMap?.Layers == null || Map.TileMap.Layers.Count == 0)
                return false;

            Vector2 tileIndex = GetTileIndex(borderPosition);
            var layer = Map.TileMap.Layers.Values.FirstOrDefault();
            
            if (layer == null || tileIndex.X < 0 || tileIndex.Y < 0 || 
                tileIndex.X >= layer.Width || tileIndex.Y >= layer.Height)
                return false;

            int? tileId = layer.GetTile((int)tileIndex.X, (int)tileIndex.Y);
            return tileId.HasValue && Map.PasableTileIds.Contains(tileId.Value);
        }
        
        private Vector2 GetTileIndex(Vector2 position)
        {
            if (Map?.TileMap == null)
                return Vector2.Zero;

            int tileX = (int)(position.X / (Map.TileMap.TileWidth * Zoom));
            int tileY = (int)(position.Y / (Map.TileMap.TileHeight * Zoom));
            return new Vector2(tileX, tileY);
        }

        public Vector2 GetTargetTileIndex(Vector2 position, Vector2 direction)
        {
            float playerCenterX = LeftBorder + (RightBorder - LeftBorder) / 2;
            float playerCenterY = UpBorder + (DownBorder - UpBorder) / 2;

            float targetX = playerCenterX;
            float targetY = playerCenterY;

            if (direction.X > 0) targetX = RightBorder + 1;
            else if (direction.X < 0) targetX = LeftBorder - 1;
            else if (direction.Y > 0) targetY = DownBorder + 1;
            else if (direction.Y < 0) targetY = UpBorder - 1;

            return GetTileIndex(new Vector2(targetX, targetY));
        }

        private void UpdateAnimation(float deltaTime)
        {
            AnimationTimer += deltaTime;
            if (AnimationTimer >= AnimationSpeed)
            {
                AnimationTimer = 0;
                Frame = Ascending ? Frame + 1 : Frame - 1;
                Ascending = Frame != 2 && (Frame == 0 || Ascending);
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            var sourceRect = new Rectangle(Frame * SpriteWidth, CurrentRow * SpriteHeight, SpriteWidth, SpriteHeight);
            spriteBatch.Draw(spriteSheet, Position, sourceRect, Color.White, 0, Vector2.Zero, Zoom, SpriteEffect, 0);
        }

        public void UseToolOrSeed()
        {
            if (SelectedItem < 0 || SelectedItem >= ActionBar.Count || ActionBar[SelectedItem] == null)
                return;

            if (!(Map is PlayerFarm playerFarm))
            {
                Console.WriteLine("Tools and seeds can only be used on the farm!");
                return;
            }

            var selectedItem = ActionBar[SelectedItem];
            Vector2 tileIndex = GetTargetTileIndex(Position, Direction);
            var layer = Map.TileMap.Layers.Values.FirstOrDefault();
            
            if (layer == null || tileIndex.X >= layer.Width || tileIndex.Y >= layer.Height)
                return;

            int currentTileId = layer.GetTile((int)tileIndex.X, (int)tileIndex.Y);
            int newTileId = currentTileId;

            switch (selectedItem)
            {
                case Seed seed:
                    HandleSeedPlanting(seed, currentTileId, tileIndex, layer, ref newTileId);
                    break;
                case Tool tool:
                    string itemName = tool.Name?.ToLower() ?? "";
                    HandleToolUsage(itemName, currentTileId, ref newTileId);
                    break;
                default:
                    break;
            }

            if (newTileId != currentTileId)
            {
                playerFarm.ModifyTile((int)tileIndex.X, (int)tileIndex.Y, newTileId);
            }
        }

        public void HarvestCrop()
        {
            Vector2 tileIndex = GetTargetTileIndex(Position, Direction);
            var layer = Map.TileMap.Layers.Values.FirstOrDefault();
            
            int targetTileId = layer.GetTile((int)tileIndex.X, (int)tileIndex.Y);
            
            if (targetTileId == 95)
            {
                var plant = PlantGrowthSystem.Instance.GetPlantAt((int)tileIndex.X, (int)tileIndex.Y);
                
                if (plant != null)
                {
                    ((PlayerFarm)Map).ModifyTile((int)tileIndex.X, (int)tileIndex.Y, 13);
                    
                    Random random = new Random();
                    int amountToHarvest = random.Next(1, 4);
                    
                    var crop = Crop.CreateCrop(plant.Name);
                    
                    if (_contentManager != null)
                    {
                        crop.LoadContent(_contentManager);
                    }
                    
                    AddItemToInventory(crop, amountToHarvest);
                    
                    PlantGrowthSystem.Instance.RemovePlant((int)tileIndex.X, (int)tileIndex.Y);
                    
                    Console.WriteLine($"Harvested {amountToHarvest}x {plant.Name}!");
                }
            }
        }

        private void HandleSeedPlanting(Seed seed, int currentTileId, Vector2 tileIndex, dynamic layer, ref int newTileId)
        {
            if (currentTileId == 127 || currentTileId == 128)
            {
                newTileId = currentTileId switch
                {
                    127 => 111,
                    128 => 112,
                    _ => currentTileId
                };

                string seedName = seed.Name;
                if (seedName.EndsWith(" seed"))
                {
                    seedName = seedName.Substring(0, seedName.Length - 5);
                }

                PlantGrowthSystem.Instance.PlantSeed(seedName, (int)tileIndex.X, (int)tileIndex.Y, newTileId);
                
                if (seed.Amount > 1)
                {
                    seed.DecreaseAmount();
                }
                else
                {
                    ActionBar[SelectedItem] = null;
                }
            }
        }

        private void HandleToolUsage(string toolName, int currentTileId, ref int newTileId)
        {
            if (toolName.Contains("shovel"))
            {
                newTileId = currentTileId switch
                {
                    1 => 13,
                    13 => 1,
                    _ => currentTileId
                };
            }
            else if (toolName.Contains("hoe"))
            {
                newTileId = currentTileId switch
                {
                    13 => 127,
                    127 => 13,
                    _ => currentTileId
                };
            }
            else if (toolName.Contains("watering can"))
            {
                newTileId = currentTileId switch
                {
                    127 => 128,
                    111 => 112,
                    79 => 80,
                    _ => currentTileId
                };
                
                if (currentTileId == 111)
                {
                    Vector2 tileIndex = GetTargetTileIndex(Position, Direction);
                    var plant = PlantGrowthSystem.Instance.GetPlantAt((int)tileIndex.X, (int)tileIndex.Y);
                    if (plant != null)
                    {
                        plant.isWatered = true;
                    }
                }
            }
        }

        private void InitializeBasicTools()
        {
            while (ActionBar.Count < 10)
            {
                ActionBar.Add(null);
            }
    
            var basicTools = Tool.CreateBasicToolset();
            for (int i = 0; i < basicTools.Count && i < ActionBar.Count; i++)
            {
                ActionBar[i] = basicTools[i];
            }
        }
        
        public void InitializeSeeds()
        {
            var carrotSeed = Seed.CreateSeed("carrot");
            AddItemToActionBar(carrotSeed, 4);
            for (int i = 0; i < 10; i++) carrotSeed.IncreaseAmount();
            Console.WriteLine(carrotSeed.Name);
            Console.WriteLine(ActionBar[4].Name);
        }
        
        public void AddItemToInventory(Item item, int amount)
        {
            // First try to stack with existing items in ACTION BAR
            for (int i = 0; i < ActionBar.Count; i++)
            {
                var actionBarItem = ActionBar[i];
                if (actionBarItem != null && actionBarItem.Name == item.Name && 
                    actionBarItem.Amount < actionBarItem.StackSize)
                {
                    int spaceAvailable = actionBarItem.StackSize - actionBarItem.Amount;
                    int amountToAdd = Math.Min(amount, spaceAvailable);
                    
                    actionBarItem.Amount += amountToAdd;
                    amount -= amountToAdd;
                    
                    Console.WriteLine($"Stacked {amountToAdd}x {item.Name} with action bar slot {i}.");
                    
                    if (amount <= 0) return; // All items stacked successfully
                }
            }
            
            // Then try to stack with existing items in INVENTORY
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
                    
                    Console.WriteLine($"Stacked {amountToAdd}x {item.Name} with inventory slot {i}.");
                    
                    if (amount <= 0) return; // All items stacked successfully
                }
            }

            // Find first empty slot for remaining items
            while (amount > 0)
            {
                int emptySlot = FindFirstEmptyInventorySlot();
                if (emptySlot == -1)
                {
                    Console.WriteLine("Inventory is full! Cannot add more items.");
                    return;
                }

                // Ensure inventory list is large enough
                while (Inventory.Count <= emptySlot)
                {
                    Inventory.Add(null);
                }

                // Create new item for this slot
                var newItem = CreateNewItemCopy(item);
                newItem.Amount = Math.Min(amount, item.StackSize);
                
                if (_contentManager != null)
                {
                    newItem.LoadContent(_contentManager);
                }

                Inventory[emptySlot] = newItem;
                amount -= newItem.Amount;
                
                Console.WriteLine($"Added {newItem.Amount}x {newItem.Name} to inventory slot {emptySlot}.");
            }
        }

        private Item CreateNewItemCopy(Item original)
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
                return new Tool(tool.Name, tool.Description, tool.IconPath, tool.StackSize, tool.Price, tool.Durability, tool.TypeOfTool);
            }
            
            throw new NotImplementedException($"Cannot create copy of item type: {original.GetType()}");
        }
        
        public void AddItemToActionBar(Item item, int index)
        {
            if(index < 10)
                if (ActionBar[index] == null)
                {
                    ActionBar.Insert(index, item);
                }
                else
                {
                    Debug.WriteLine("There is already item at the given index");
                }
            else Debug.WriteLine("Index out of bounds for action bar!");
        }
        
        public void PrintInventory()
        {
            Console.WriteLine("=== PLAYER INVENTORY ===");
            Console.WriteLine($"Items: {Inventory.Count}/30");
            Console.WriteLine("------------------------");
    
            if (Inventory.Count == 0)
            {
                Console.WriteLine("Inventory is empty!");
                return;
            }
    
            for (int i = 0; i < Inventory.Count; i++)
            {
                var item = Inventory[i];
                if (item != null)
                {
                    Console.WriteLine($"[{i:D2}] {item.Name} x {item.Amount}");
                }
                else
                {
                    Console.WriteLine($"[{i:D2}] NULL");
                }
            }
            Console.WriteLine("========================");
        }

        public void SwapActionBarItems(int index1, int index2)
        {
            if (index1 < 0 || index1 >= ActionBar.Count || index2 < 0 || index2 >= ActionBar.Count)
            {
                Console.WriteLine($"Invalid action bar indices: {index1}, {index2}");
                return;
            }

            var temp = ActionBar[index1];
            ActionBar[index1] = ActionBar[index2];
            ActionBar[index2] = temp;

            Console.WriteLine($"Swapped action bar items: [{index1}] <-> [{index2}]");
        }

        public void SwapInventoryItems(int index1, int index2)
        {
            while (Inventory.Count <= Math.Max(index1, index2))
            {
                Inventory.Add(null);
            }

            var temp = Inventory[index1];
            Inventory[index1] = Inventory[index2];
            Inventory[index2] = temp;

            Console.WriteLine($"Swapped inventory items: [{index1}] <-> [{index2}]");
        }

        public void MoveInventoryToActionBar(int inventoryIndex, int actionBarIndex)
        {
            if (actionBarIndex < 0 || actionBarIndex >= ActionBar.Count)
            {
                Console.WriteLine($"Invalid action bar index: {actionBarIndex}");
                return;
            }

            while (Inventory.Count <= inventoryIndex)
            {
                Inventory.Add(null);
            }

            var inventoryItem = Inventory[inventoryIndex];
            var actionBarItem = ActionBar[actionBarIndex];

            ActionBar[actionBarIndex] = inventoryItem;
            Inventory[inventoryIndex] = actionBarItem;

            Console.WriteLine($"Moved from inventory[{inventoryIndex}] to action bar[{actionBarIndex}]");
        }

        public void MoveActionBarToInventory(int actionBarIndex, int inventoryIndex)
        {
            if (actionBarIndex < 0 || actionBarIndex >= ActionBar.Count)
            {
                Console.WriteLine($"Invalid action bar index: {actionBarIndex}");
                return;
            }

            while (Inventory.Count <= inventoryIndex)
            {
                Inventory.Add(null);
            }

            var actionBarItem = ActionBar[actionBarIndex];
            var inventoryItem = Inventory[inventoryIndex];

            Inventory[inventoryIndex] = actionBarItem;
            ActionBar[actionBarIndex] = inventoryItem;

            Console.WriteLine($"Moved from action bar[{actionBarIndex}] to inventory[{inventoryIndex}]");
        }

        public int FindFirstEmptyInventorySlot()
        {
            for (int i = 0; i < 30; i++)
            {
                if (i >= Inventory.Count || Inventory[i] == null)
                {
                    return i;
                }
            }
            return -1;
        }

        public int FindFirstEmptyActionBarSlot()
        {
            for (int i = 0; i < ActionBar.Count; i++)
            {
                if (ActionBar[i] == null)
                {
                    return i;
                }
            }
            return -1;
        }

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
    }
}