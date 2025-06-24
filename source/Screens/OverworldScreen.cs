using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using FontStashSharp;
using Myra.Graphics2D.UI;
using Moonlight_Vale.Entity;
using Moonlight_Vale.Screens.Maps;
using Moonlight_Vale.Systems;
using Moonlight_Vale.UI;
using Squared.Tiled;
using System.Linq;
using System.Threading;
using Moonlight_Vale.Entity.Items;

namespace Moonlight_Vale.Screens
{
    public class OverworldScreen : GameScreen
    {
        public bool isInGameMenuActive;
        public bool isHUDActive;
        public bool isDevToolsActive;
        public bool isMouseOverlayingHUD;
        public bool newGame;

        public IMap CurrentMap { get; private set; }
        public Player Player { get; private set; }
        public Camera2D Camera { get; private set; }
        public FontSystem FontSystem { get; private set; }
        public Desktop Desktop { get; private set; }
        public HudManager HudManager { get; private set; }
        public LogWindow LogWindow { get; private set; }
        public SavingSystem SavingSystem => SavingSystem.Instance;
        private Texture2D TargetSprite;
        public float Zoom { get; private set; } = 2.0f;
        public KeyboardState previousKeyboardState { get; private set; }
        public MouseState previousMouseState { get; private set; }

        public OverworldScreen(MoonlightVale game, ScreenManager manager, SpriteBatch batch, Desktop desktop, bool newGame)
            : base(game, manager, batch, desktop)
        {
            FontSystem = game._fontSystem;
            Desktop = desktop;
            isInGameMenuActive = false;
            isHUDActive = true;
            isDevToolsActive = false;
            isMouseOverlayingHUD = false;
            this.newGame = newGame;
        }

        public override void Initialize()
        {
            PlantData.LoadFromJson(content.RootDirectory + @"\Data\plants.json");
            TimeSystem.Instance.Start();
            
            // Subscribe to player collapse event
            TimeSystem.Instance.PlayerCollapsedFromExhaustion += OnPlayerCollapsedFromExhaustion;
            
            Camera = new Camera2D();
            CurrentMap = new PlayerFarm(this);
            Player = new Player(CurrentMap.PlayerSpawnPoint, CurrentMap, this);
            HudManager = new HudManager(this);
            HudManager.Initialize();
            
            previousKeyboardState = Keyboard.GetState();
            previousMouseState = Mouse.GetState();
        }

        private void OnPlayerCollapsedFromExhaustion(Vector2 spawnPosition)
        {
            Console.WriteLine("Player collapsed from exhaustion! Moving to PlayerHouse...");
            
            SwitchToHouse();
            
            // Set player position to the exact spawn coordinates from TimeSystem (142, 120)
            Player.Position = spawnPosition;
            
            // Update camera to follow player at new position
            Camera.Position = Player.Position - new Vector2(1920 / 2f / Zoom - Player.SpriteWidth * Zoom / 2f,
                1080 / 2f / Zoom - Player.SpriteHeight * Zoom / 2f);
            
            Console.WriteLine($"Player spawned at home: {spawnPosition}");
        }
        

        public override void LoadContent(ContentManager content)
        {
            CurrentMap.LoadContent(content);
            Player.LoadContent(content, @"Spritesheets\\hero_spritesheet");
            TargetSprite = content.Load<Texture2D>(@"Spritesheets\\tile_cursor2");

            // Initialize LogWindow with FontSystem
            LogWindow = new LogWindow(game.GraphicsDevice,  1920, 1080);

            newGame = false;
        }

        public void SaveGame() { }
        public void LoadGame() { }
        public void ReturnToMenu()
        {   
            TimeSystem.Instance.Stop();
            screenManager.RemoveScreen();
            screenManager.AddScreen(new MainMenuScreen(game, screenManager, spriteBatch, Desktop, FontSystem));
        }

        public void ExitGame()
        {
            screenManager.RemoveScreen();
            game.Exit();
        }

        public override void Update(GameTime gameTime)
        {
            var keyboard = Keyboard.GetState();
            var mouse = Mouse.GetState();

            Camera.Zoom = Zoom;
            Player.Zoom = Zoom;

            Player.Update(gameTime, keyboard, mouse, previousMouseState);

            Camera.Position = Player.Position - new Vector2(1920 / 2f / Zoom - Player.SpriteWidth * Zoom / 2f,
                1080 / 2f / Zoom - Player.SpriteHeight * Zoom / 2f);

            if (keyboard.IsKeyDown(Keys.Escape) && previousKeyboardState.IsKeyUp(Keys.Escape))
                isInGameMenuActive = !isInGameMenuActive;

            if (keyboard.IsKeyDown(Keys.OemTilde) && previousKeyboardState.IsKeyUp(Keys.OemTilde))
                isDevToolsActive = !isDevToolsActive;

            if (keyboard.IsKeyDown(Keys.LeftAlt) && keyboard.IsKeyDown(Keys.Z) &&
                previousKeyboardState.IsKeyUp(Keys.Z))
                isHUDActive = !isHUDActive;
                
            if (keyboard.IsKeyDown(Keys.B) && previousKeyboardState.IsKeyUp(Keys.B))
            {
                HudManager.ToggleBackpackWindow();
            }
            if (keyboard.IsKeyDown(Keys.I) && !previousKeyboardState.IsKeyDown(Keys.I)) // Jednokrotne naciśnięcie I
            {
                Player.PrintInventory();
            }

            HudManager.Update();
            HudManager.UpdateTime();
            HudManager.UpdateItemBarSelection(Player.SelectedItem);
            HudManager.UpdateVisibility(isHUDActive, isInGameMenuActive);
            HudManager.UpdateItemBarIcons();
            HudManager.UpdateTooltip();
            
            // Always update LogWindow to capture console output
            LogWindow.Update(gameTime);

            isMouseOverlayingHUD = HudManager.IsMouseHoveringAnyWidget(new Point(mouse.X, mouse.Y));

            HandleMapTransitions(mouse);

            previousKeyboardState = keyboard;
            previousMouseState = mouse;
        }

        private void HandleMapTransitions(MouseState mouse)
        {
            if (CurrentMap?.TileMap?.Layers == null || CurrentMap.TileMap.Layers.Count == 0)
                return;

            Vector2 tileIndex = GetTileIndex(Player.Position);
            var layer = CurrentMap.TileMap.Layers.Values.FirstOrDefault();

            if (layer == null || tileIndex.X < 0 || tileIndex.Y < 0 ||
                tileIndex.X >= layer.Width || tileIndex.Y >= layer.Height)
                return;

            int? tileId = layer.GetTile((int)tileIndex.X, (int)tileIndex.Y);

            if (mouse.RightButton == ButtonState.Pressed && previousMouseState.RightButton == ButtonState.Released && !isMouseOverlayingHUD)
            {
                if (CurrentMap is PlayerFarm && tileId is 83 or 82 && Player.Position.X is > 520 and < 560)
                {
                    SwitchToHouse();
                }
                else if (CurrentMap is PlayerHouse)
                {
                    // Check for sleeping (bed area)
                    if (Player.Position.X > 128 && Player.Position.X < 130 && Player.Position.Y > 105 && Player.Position.Y < 165)
                    {
                        TimeSystem.Instance.StartSleeping();
                    }
                    // Check for exit (door area)
                    if (Player.Position.X is > 219 and < 265 && Player.Position.Y >= 364)
                    {
                        SwitchToFarm();
                    }
                }
            }
            if (CurrentMap is PlayerFarm && Player.Position.X > 1360 && (Player.Position.Y > 680 && Player.Position.Y < 688))
            {
                SwitchToTown();
            }
            else if (CurrentMap is Town &&  Player.Position.X is < 8  && Player.Position.Y is > 680 and < 718) // SWITCH LATER
            {
                SwitchToFarm();
            }
        }

        public void SwitchToHouse()
        {
            // No need to save farm state - changes are already saved immediately
            CurrentMap = new PlayerHouse(this);
            Player.Map = CurrentMap;
            CurrentMap.LoadContent(game.Content);
            Player.Position = CurrentMap.PlayerSpawnPoint;
        }

        public void SwitchToFarm()
        {
            if (CurrentMap is PlayerHouse)
            {
                Player.Position = new Vector2(550, 520);
            }
            else
            {
                Player.Position = new Vector2(1347, 687);
            }
            CurrentMap = new PlayerFarm(this);
            Player.Map = CurrentMap;
            CurrentMap.LoadContent(game.Content);
            
        }

        public void SwitchToTown()
        {
            // 1. Clear previous map reference immediately
            
            CurrentMap = null; // Prevent rendering old map
    
            // 2. Create and load new map
            var newMap = new Town(this);
            newMap.LoadContent(game.Content);
    
            // 3. Set player position BEFORE assigning map
            Player.Position = new Vector2(20, 687);
    
            // 4. Update camera position immediately to prevent jump
            Camera.Position = Player.Position - new Vector2(
                1920 / 2f / Zoom - Player.SpriteWidth * Zoom / 2f,
                1080 / 2f / Zoom - Player.SpriteHeight * Zoom / 2f
            );
    
            // 5. Assign new map atomically
            CurrentMap = newMap;
            Player.Map = CurrentMap;
    
            // 6. Force immediate camera update
            Camera.Zoom = Zoom;
    
            Console.WriteLine($"Switched to Town - Player: {Player.Position}, Camera: {Camera.Position}");
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            graphicsDevice.Clear(Color.Black);
            if (CurrentMap?.TileMap == null) return;

            spriteBatch.Begin(transformMatrix: Camera.GetViewMatrix(), samplerState: SamplerState.PointClamp);

            foreach (var layer in CurrentMap.TileMap.Layers.Values)
            {
                DrawLayer(spriteBatch, layer);
            }

            DrawTargetTile(spriteBatch);
            Player.Draw(spriteBatch);
            spriteBatch.End();

            if (isDevToolsActive)
            {
                DrawDebugInfo(spriteBatch);
                
                // Draw LogWindow as part of dev tools
                spriteBatch.Begin();
                LogWindow.Draw(spriteBatch);
                spriteBatch.End();
            }

            HudManager.Draw();
        }

        private void DrawDebugInfo(SpriteBatch spriteBatch)
        {
            spriteBatch.Begin();
            var font = FontSystem.GetFont(4);

            font.DrawText(spriteBatch, $"Player position: {(int)Player.Position.X}, {(int)Player.Position.Y}", new Vector2(20, 50), Color.White);

            if (CurrentMap?.TileMap != null)
            {
                // Calculate center position of player - bigger Y offset to show the tile player is standing on
                Vector2 playerCenter = new Vector2(Player.Position.X + 16, Player.Position.Y + 24);
                Vector2 tileIndex = GetTileIndex(playerCenter);

                font.DrawText(spriteBatch, $"Tile index (center): {tileIndex.X}, {tileIndex.Y}", new Vector2(20, 100), Color.White);

                var layer = CurrentMap.TileMap.Layers.Values.FirstOrDefault();
                if (layer != null && tileIndex.X >= 0 && tileIndex.Y >= 0 &&
                    tileIndex.X < layer.Width && tileIndex.Y < layer.Height)
                {
                    int? tileId = layer.GetTile((int)tileIndex.X, (int)tileIndex.Y);
                    font.DrawText(spriteBatch, $"Tile ID (center): {tileId}", new Vector2(20, 150), Color.White);
                }
            }

            font.DrawText(spriteBatch, $"Is HUD active: {isHUDActive}", new Vector2(20, 200), Color.White);
            
            string itemName;
            try
            {
                itemName = Player.ActionBar[Player.SelectedItem].Name;
            }
            catch (NullReferenceException)
            {
                itemName = "Empty";
            }

            font.DrawText(spriteBatch, $"Selected Item: {Player.SelectedItem}, {itemName}", new Vector2(20, 250), Color.White);
            font.DrawText(spriteBatch, $"Current Map: {CurrentMap?.GetType().Name}", new Vector2(20, 300), Color.White);
            font.DrawText(spriteBatch, $"UpBorder: {(int)Player.UpBorder}", new Vector2(20, 350), Color.White);
            font.DrawText(spriteBatch, $"DownBorder: {(int)Player.DownBorder}", new Vector2(20, 400), Color.White);
            font.DrawText(spriteBatch, $"LeftBorder: {(int)Player.LeftBorder}", new Vector2(20, 450), Color.White);
            font.DrawText(spriteBatch, $"RightBorder: {(int)Player.RightBorder}", new Vector2(20, 500), Color.White);
            font.DrawText(spriteBatch, $"Mouse over HUD: {isMouseOverlayingHUD}", new Vector2(20, 550), Color.White);

            font = FontSystem.GetFont(3);
            DrawPlayerHitbox(spriteBatch);
            DrawPlantNameOnHover(spriteBatch, font);

            spriteBatch.End();
        }

        private void DrawPlantNameOnHover(SpriteBatch spriteBatch, SpriteFontBase font)
        {
            var mouse = Mouse.GetState();
            Vector2 mouseScreenPos = new Vector2(mouse.X, mouse.Y);
            
            // Convert mouse screen position to world position
            Matrix inverseViewMatrix = Matrix.Invert(Camera.GetViewMatrix());
            Vector2 mouseWorldPos = Vector2.Transform(mouseScreenPos, inverseViewMatrix);
            
            // Get tile index from mouse world position
            Vector2 mouseTileIndex = GetTileIndex(mouseWorldPos);
            
            if (CurrentMap?.TileMap?.Layers == null || CurrentMap.TileMap.Layers.Count == 0)
                return;
                
            var layer = CurrentMap.TileMap.Layers.Values.FirstOrDefault();
            if (layer == null || mouseTileIndex.X < 0 || mouseTileIndex.Y < 0 ||
                mouseTileIndex.X >= layer.Width || mouseTileIndex.Y >= layer.Height)
                return;
                
            // Check if the tile has planted seeds (ID 111 or 112)
            int? tileId = layer.GetTile((int)mouseTileIndex.X, (int)mouseTileIndex.Y);
            var expectedTileIds = new List<int?> {111, 112, 79, 80, 85, 95 }; // dry seeds, watered seeds, growing plants, ready to harvest
            if (expectedTileIds.Contains(tileId))
            {
                // Get plant from PlantGrowthSystem
                var plant = PlantGrowthSystem.Instance.GetPlantAt((int)mouseTileIndex.X, (int)mouseTileIndex.Y);
                if (plant != null)
                {
                    // Create multi-line plant info using plant's properties and methods
                    string plantInfo = $"{plant.Name}\n" +
                                     $"Planted on day: {plant.PlantingDay}\n" +
                                     $"Days to grow: {plant.GetDaysRemainingToGrow()}\n" +
                                     $"Watered: {(plant.isWatered ? "Yes" : "No")}";
                    
                    Vector2 textPosition = new Vector2(mouse.X + 10, mouse.Y - 80);
                    
                    // Calculate background size for multi-line text
                    string[] lines = plantInfo.Split('\n');
                    float maxWidth = 0;
                    foreach (string line in lines)
                    {
                        float lineWidth = font.MeasureString(line).X;
                        if (lineWidth > maxWidth)
                            maxWidth = lineWidth;
                    }
                    
                    int lineHeight = 20; // Approximate line height
                    var backgroundRect = new Rectangle((int)textPosition.X - 5, (int)textPosition.Y - 5, 
                        (int)maxWidth + 10, lines.Length * lineHeight + 10);
                    
                    var backgroundTexture = new Texture2D(graphicsDevice, 1, 1);
                    backgroundTexture.SetData(new Color[] { new Color(0, 0, 0, 180) });
                    spriteBatch.Draw(backgroundTexture, backgroundRect, Color.White);
                    
                    // Draw each line of plant info
                    for (int i = 0; i < lines.Length; i++)
                    {
                        Vector2 linePosition = new Vector2(textPosition.X, textPosition.Y + i * lineHeight);
                        font.DrawText(spriteBatch, lines[i], linePosition, Color.White);
                    }
                }
            }
        }

        private void DrawPlayerHitbox(SpriteBatch spriteBatch)
        {
            var _texture = new Texture2D(graphicsDevice, 1, 1);
            _texture.SetData(new Color[] { new(100, 250, 100, 75) });

            Vector2 worldPosition = new Vector2(Player.LeftBorder, Player.UpBorder);
            Vector2 screenPosition = Vector2.Transform(worldPosition, Camera.GetViewMatrix());

            spriteBatch.Draw(_texture, new Rectangle((int)screenPosition.X, (int)screenPosition.Y,
                    (int)(Player.SpriteWidth * Zoom * Player.Zoom),
                    (int)((Player.SpriteHeight - (int)Player.HeadOffset) * Zoom * Player.Zoom)),
                Color.White);
        }

        private void DrawTargetTile(SpriteBatch spriteBatch)
        {
            var keyboard = Keyboard.GetState();
            if (!keyboard.IsKeyDown(Keys.LeftControl) || CurrentMap?.TileMap == null)
                return;

            Vector2 targetTileIndex = Player.GetTargetTileIndex(Player.Position, Player.Direction);

            if (targetTileIndex.X < 0 || targetTileIndex.Y < 0)
                return;

            int tileSize = CurrentMap.TileMap.TileWidth;
            Vector2 tileWorldPos = targetTileIndex * tileSize * 2;

            spriteBatch.Draw(TargetSprite, new Rectangle((int)tileWorldPos.X, (int)tileWorldPos.Y,
                tileSize * 2, tileSize * 2), Color.White);
        }

        private void DrawLayer(SpriteBatch spriteBatch, Layer layer)
        {
            if (CurrentMap?.TileMap?.Tilesets == null || !CurrentMap.TileMap.Tilesets.Any())
                return;

            int scaledTileSize = (int)(CurrentMap.TileMap.TileWidth * Zoom);

            for (int y = 0; y < layer.Height; y++)
            for (int x = 0; x < layer.Width; x++)
            {
                int tileIndex = layer.GetTile(x, y);
                var tileRect = new Rectangle();
                if (!CurrentMap.TileMap.Tilesets.First().Value.MapTileToRect(tileIndex, ref tileRect)) continue;

                spriteBatch.Draw(CurrentMap.TileSet, new Vector2(x, y) * scaledTileSize, tileRect, Color.White, 0, Vector2.Zero, Zoom, SpriteEffects.None, 0);
            }
        }

        public override void Unload()
        {
            // Dispose LogWindow to restore console
            LogWindow?.Dispose();
            
            Desktop.Widgets.Clear();
            Desktop.Root = null;
            // Unsubscribe from collapse event to prevent memory leaks
            TimeSystem.Instance.PlayerCollapsedFromExhaustion -= OnPlayerCollapsedFromExhaustion;
            TimeSystem.Instance.Dispose();
        }

        public Vector2 GetTileIndex(Vector2 position)
        {
            if (CurrentMap?.TileMap == null)
                return Vector2.Zero;

            int tileX = (int)(position.X / (CurrentMap.TileMap.TileWidth * Zoom));
            int tileY = (int)(position.Y / (CurrentMap.TileMap.TileHeight * Zoom));
            return new Vector2(tileX, tileY);
        }
    }

    public class Camera2D
    {
        public Vector2 Position { get; set; }
        public float Zoom { get; set; } = 1.0f;

        public Matrix GetViewMatrix()
        {
            return Matrix.CreateTranslation(new Vector3(-Position, 0.0f)) *
                   Matrix.CreateScale(Zoom);
        }
    }
}