using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Moonlight_Vale.Entity.Items;
using Moonlight_Vale.Screens;
using Moonlight_Vale.Screens.Maps;
using System.Linq;

namespace Moonlight_Vale.Entity
{
    public class Player
    {
        private const int SPRITE_WIDTH = 16;
        private const int SPRITE_HEIGHT = 24;
        private const float HEAD_OFFSET = 12f;
        private const float DEFAULT_SPEED = 5000f;
        private const float SPRINT_MULTIPLIER = 1.8f;
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
            Position = startPosition;
            Map = map;
            this.overworldScreen = overworldScreen;
            UpdateBorders();
            InitializeBasicTools();
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
                   UseTool();
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

        public void UseTool()
        {
            // Check if there's a selected item in the action bar
            if (SelectedItem < 0 || SelectedItem >= ActionBar.Count || ActionBar[SelectedItem] == null)
                return;

            var selectedTool = ActionBar[SelectedItem];
            Vector2 tileIndex = GetTargetTileIndex(Position, Direction);
            var layer = Map.TileMap.Layers.Values.FirstOrDefault();
            
            if (layer == null || tileIndex.X >= layer.Width || tileIndex.Y >= layer.Height)
                return;

            int currentTileId = layer.GetTile((int)tileIndex.X, (int)tileIndex.Y);
            
            string toolName = selectedTool?.Name?.ToLower() ?? "";

            int newTileId = currentTileId;

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
                    13 => 127,  
                    127 => 13, 
                    _ => currentTileId
                };
            }

            if (newTileId != currentTileId)
            {
                layer.Tiles[(int)(tileIndex.Y * layer.Width + tileIndex.X)] = newTileId;
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
    }
}