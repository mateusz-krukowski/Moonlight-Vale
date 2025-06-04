using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Moonlight_Vale.Screens;
using Squared.Tiled;

namespace Moonlight_Vale.Entity
{
    public class Player
    {
        private const int SPRITE_WIDTH = 16;
        private const int SPRITE_HEIGHT = 24;
        private const float HEAD_OFFSET = 12f;
        private const float DEFAULT_SPEED = 3200f;
        private const float ANIMATION_SPEED = 0.18f;

        private Texture2D spriteSheet;
        private SpriteEffects spriteEffect;
        private Map map;
        private int frame;
        private float animationTimer;
        private int currentRow;
        private bool ascending = true;
        private int selectedItem;

        public Player(Vector2 startPosition, Map map)
        {
            Position = startPosition;
            frame = 1;
            animationTimer = 0;
            currentRow = 0;
            spriteEffect = SpriteEffects.None;
            this.map = map;
            UpdateBorders();
        }

        public OverworldScreen OverworldScreen { get; set; }
        public Vector2 Position { get; set; }
        public Vector2 Velocity { get; set; }
        public Vector2 Direction { get; private set; }
        public float Zoom { get; set; } = 2.0f;
        public float Speed { get; set; } = DEFAULT_SPEED;
        public int SpriteWidth => SPRITE_WIDTH;
        public int SpriteHeight => SPRITE_HEIGHT;
        public float HeadOffset => HEAD_OFFSET;
        public float AnimationSpeed => ANIMATION_SPEED;
        public float UpBorder { get; private set; }
        public float DownBorder { get; private set; }
        public float LeftBorder { get; private set; }
        public float RightBorder { get; private set; }

        public int SelectedItem
        {
            get => selectedItem;
            set => selectedItem = value < 10 && value >= 0 ? value : 0;
        }

        public void LoadContent(ContentManager content, string spriteSheetPath)
        {
            spriteSheet = content.Load<Texture2D>(spriteSheetPath);
        }

        public void Update(GameTime gameTime, KeyboardState keyboard)
        {
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            Velocity = Vector2.Zero;

            // Handle movement
            switch (keyboard)
            {
                case var k when k.IsKeyDown(Keys.W):
                    Move(Vector2.UnitY * -Speed, deltaTime, 2);
                    break;
                case var k when k.IsKeyDown(Keys.S):
                    Move(Vector2.UnitY * Speed, deltaTime, 0);
                    break;
                case var k when k.IsKeyDown(Keys.A):
                    Move(Vector2.UnitX * -Speed, deltaTime, 1, SpriteEffects.None);
                    break;
                case var k when k.IsKeyDown(Keys.D):
                    Move(Vector2.UnitX * Speed, deltaTime, 1, SpriteEffects.FlipHorizontally);
                    break;
                default:
                    frame = 1;
                    break;
            }

            // Handle item selection
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

            // Handle tile replacement
            if (keyboard.IsKeyDown(Keys.E))
            {
                HandleTileReplacement();
            }
        }

        private void Move(Vector2 direction, float deltaTime, int row, SpriteEffects effect = SpriteEffects.None)
        {
            spriteEffect = effect;
            currentRow = row;

            Direction = direction != Vector2.Zero ? Vector2.Normalize(direction) : Direction;

            Vector2 originalPosition = Position;
            Vector2 newPosition = originalPosition;
            float scaledHeadOffset = HeadOffset * Zoom;

            // Handle horizontal movement
            if (direction.X != 0)
            {
                float newX = originalPosition.X + direction.X * deltaTime;
                float tempLeft = newX;
                float tempRight = newX + (SpriteWidth * Zoom);

                float currentUp = originalPosition.Y + scaledHeadOffset;
                float currentDown = originalPosition.Y + (SpriteHeight * Zoom);

                bool canMove = true;
                if (direction.X < 0) // Moving left
                {
                    canMove = CanMoveToTile(new Vector2(tempLeft, currentUp)) && 
                              CanMoveToTile(new Vector2(tempLeft, currentDown));
                }
                else if (direction.X > 0) // Moving right
                {
                    canMove = CanMoveToTile(new Vector2(tempRight, currentUp)) && 
                              CanMoveToTile(new Vector2(tempRight, currentDown));
                }

                if (canMove)
                {
                    newPosition.X = newX;
                }
            }

            // Handle vertical movement
            if (direction.Y != 0)
            {
                float newY = originalPosition.Y + direction.Y * deltaTime;
                float tempUp = newY + scaledHeadOffset;
                float tempDown = newY + (SpriteHeight * Zoom);

                float currentLeft = newPosition.X;
                float currentRight = newPosition.X + (SpriteWidth * Zoom);

                bool canMove = true;
                if (direction.Y < 0) // Moving up
                {
                    canMove = CanMoveToTile(new Vector2(currentLeft, tempUp)) && 
                              CanMoveToTile(new Vector2(currentRight, tempUp));
                }
                else if (direction.Y > 0) // Moving down
                {
                    canMove = CanMoveToTile(new Vector2(currentLeft, tempDown)) && 
                              CanMoveToTile(new Vector2(currentRight, tempDown));
                }

                if (canMove)
                {
                    newPosition.Y = newY;
                }
            }

            Position = newPosition;
            UpdateAnimation(deltaTime);
            UpdateBorders();
        }

        private void HandleTileReplacement()
        {
            Vector2 tileIndex = GetTargetTileIndex(Position, Direction);

            if (tileIndex.X < 0 || tileIndex.Y < 0)
            {
                return;
            }

            var layer = map.Layers.Values[0];
            if (layer == null) return;

            int currentTileId = layer.GetTile((int)tileIndex.X, (int)tileIndex.Y);
            if (currentTileId > 0)
            {
                layer.Tiles[(int)(tileIndex.Y * layer.Width + tileIndex.X)] = 12;
            }
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
            Vector2 tileIndex = GetTileIndex(borderPosition);
            int? tileId = map.Layers.Values[0].GetTile((int)tileIndex.X, (int)tileIndex.Y);
            return tileId is <= 12 or 114 or 115 or 116  or 98 or 99 or 100 or 101; //114 and 115 house entrance
        }

        private Vector2 GetTileIndex(Vector2 position)
        {
            int tileX = (int)(position.X / (map.TileWidth * Zoom));
            int tileY = (int)(position.Y / (map.TileHeight * Zoom));
            return new Vector2(tileX, tileY);
        }

        private Vector2 GetTargetTileIndex(Vector2 position, Vector2 direction)
        {
            Vector2 offset = Vector2.Zero;
            if (direction.X > 0) offset = Vector2.UnitX;
            else if (direction.X < 0) offset = -Vector2.UnitX;
            else if (direction.Y > 0) offset = Vector2.UnitY;
            else if (direction.Y < 0) offset = -Vector2.UnitY;

            Vector2 currentTileIndex = GetTileIndex(position);
            currentTileIndex.Y += 1;
            return currentTileIndex + offset;
        }

        private void UpdateAnimation(float deltaTime)
        {
            animationTimer += deltaTime;
            if (animationTimer >= AnimationSpeed)
            {
                animationTimer = 0;
                frame = ascending ? frame + 1 : frame - 1;
                ascending = frame != 2 && (frame == 0 || ascending);
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            var sourceRect = new Rectangle(frame * SpriteWidth, currentRow * SpriteHeight, SpriteWidth, SpriteHeight);
            spriteBatch.Draw(spriteSheet, Position, sourceRect, Color.White, 0, Vector2.Zero, Zoom, spriteEffect, 0);
        }
    }
}