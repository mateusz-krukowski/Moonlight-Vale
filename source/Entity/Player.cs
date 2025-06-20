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
        private const float ANIMATION_SPEED = 0.18f;
        private const float TOOL_COOLDOWN = 0.1f; // Cooldown between tool uses in seconds

        private float _energy = 100f;
        private float _toolCooldownTimer = 0f;

        private Texture2D spriteSheet;

        public SpriteEffects SpriteEffect { get; private set; } = SpriteEffects.None;
        public int Frame { get; private set; } = 1;
        public float AnimationTimer { get; private set; }
        public int CurrentRow { get; private set; }
        public bool Ascending { get; private set; } = true;

        public List<Item> Inventory { get; } = new List<Item>(30);
        public List<Item> ActionBar { get; } = new List<Item>(10);

        public OverworldScreen overworldScreen { get; set; }

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
                
            } //handle continue game logic
            
            Map = map;
            this.overworldScreen = overworldScreen;
            UpdateBorders();
        }

        public void LoadContent(ContentManager content, string spriteSheetPath)
        {
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

            // Update tool cooldown timer
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

            // Check if left mouse button was just pressed (not held down) and cooldown has expired
            if (mouse.LeftButton == ButtonState.Pressed && 
                previousMouseState.LeftButton == ButtonState.Released && 
                _toolCooldownTimer <= 0)
            {
               if(!overworldScreen.isMouseOverlayingHUD) 
               {
                   UseToolOrSeed(); // Changed from UseTool() to UseToolOrSeed()
                   _toolCooldownTimer = TOOL_COOLDOWN; // Start cooldown
               }
            }

            // Check if right mouse button was just pressed (not held down) and cooldown has expired
            if (mouse.RightButton == ButtonState.Pressed && 
                previousMouseState.RightButton == ButtonState.Released && 
                _toolCooldownTimer <= 0)
            {
               if(!overworldScreen.isMouseOverlayingHUD && Map is PlayerFarm) 
               {
                   HarvestCrop();
                   _toolCooldownTimer = TOOL_COOLDOWN; // Start cooldown
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
            // Check if there's a selected item in the action bar
            if (SelectedItem < 0 || SelectedItem >= ActionBar.Count || ActionBar[SelectedItem] == null)
                return;

            // Check if we're on PlayerFarm
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
                    // Handle other items if needed
                    break;
            }

            // Apply tile change if necessary
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
            
            // Check if tile ID is 95 (mature plant)
            if (targetTileId == 95)
            {
                // Get plant data from growth system
                var plant = PlantGrowthSystem.Instance.GetPlantAt((int)tileIndex.X, (int)tileIndex.Y);
                
                if (plant != null)
                {
                    // Change tile to empty farmland (ID 12) using playerFarm.ModifyTile
                    ((PlayerFarm)Map).ModifyTile((int)tileIndex.X, (int)tileIndex.Y, 13);
                    
                    // Generate random harvest amount (1-3)
                    Random random = new Random();
                    int amountToHarvest = random.Next(1, 4); // 1-3 inclusive
                    
                    // Create crop with plant name
                    var crop = Crop.CreateCrop(plant.Name); // Using Name instead of Type
                    
                    // Add to inventory
                    AddItemToInventory(crop, amountToHarvest);
                    
                    
                    // Remove plant from growth system
                    PlantGrowthSystem.Instance.RemovePlant((int)tileIndex.X, (int)tileIndex.Y);
                    
                    Console.WriteLine($"Harvested {amountToHarvest}x {plant.Name}!");
                }
            }
        }

        private void HandleSeedPlanting(Seed seed, int currentTileId, Vector2 tileIndex, dynamic layer, ref int newTileId)
        {
            // Check if tile can be planted on
            if (currentTileId == 127 || currentTileId == 128) // tilled soil or watered soil
            {
                newTileId = currentTileId switch
                {
                    127 => 111, // tilled soil -> dry seeds
                    128 => 112, // watered soil -> watered seeds
                    _ => currentTileId
                };

                // Extract seed name (remove " seed" suffix)
                string seedName = seed.Name;
                if (seedName.EndsWith(" seed"))
                {
                    seedName = seedName.Substring(0, seedName.Length - 5); // Remove " seed"
                }

                // Plant the seed in the growth system, passing the initial tile type
                PlantGrowthSystem.Instance.PlantSeed(seedName, (int)tileIndex.X, (int)tileIndex.Y, newTileId);
                
                // Reduce seed count or remove from inventory if it's the last one
                if (seed.Amount > 1)
                {
                    seed.DecreaseAmount(); // Assuming this method exists
                }
                else
                {
                    // Remove seed from action bar if it's the last one
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
                    1 => 13,    // grass -> tilled soil
                    13 => 1,    // tilled soil -> grass
                    _ => currentTileId
                };
            }
            else if (toolName.Contains("hoe"))
            {
                newTileId = currentTileId switch
                {
                    13 => 127,  // tilled soil -> prepared soil
                    127 => 13,  // prepared soil -> tilled soil
                    _ => currentTileId
                };
            }
            else if (toolName.Contains("watering can"))
            {
                newTileId = currentTileId switch
                {
                    127 => 128, // tilled soil -> watered soil
                    111 => 112, // dry seeds -> watered seeds 
                    79 => 80, // dry plant -> watered plant
                    _ => currentTileId
                };
                
                // If watering planted seeds, update the plant's watered status
                if (currentTileId == 111) // dry seeds -> watered seeds
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
            // First, ensure ActionBar has 10 slots (fill with nulls)
            while (ActionBar.Count < 10)
            {
                ActionBar.Add(null);
            }
    
            // Then add basic tools to first slots
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
            if (Inventory.Count < 30)
            {
                var existingItem = Inventory.FirstOrDefault(i => i.Name == item.Name);
                if (existingItem == null)
                {   
                    item.Amount = amount;
                    Inventory.Add(item);
                    
                }
                else
                {
                    if (existingItem.Amount + amount <= existingItem.StackSize) //amount will not exceed stack size
                    {
                        existingItem.Amount += amount;
                    }
                    else
                    {
                        var amountToAdd = existingItem.StackSize - existingItem.Amount; //calculate how much can be added
                        existingItem.Amount += amountToAdd;
                        
                        var remainingAmount = amount - amountToAdd;
                        item.Amount = remainingAmount;
                        Inventory.Add(item);
                        
                    }
                }

                Console.WriteLine($"Added new item:{item.Name} x {amount} to inventory.");
            }
            else
            {
                Debug.WriteLine("Inventory is full!");
            }
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
                    Console.WriteLine($"[{i:D2}] {item.Name} x {item.Amount})");
                }
                else
                {
                    Console.WriteLine($"[{i:D2}] NULL");
                }
            }
            Console.WriteLine("========================");
        }
    }
}