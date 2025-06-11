using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using FontStashSharp;
using Myra.Graphics2D.UI;
using Moonlight_Vale.Entity;
using Moonlight_Vale.Screens.Maps;
using Moonlight_Vale.Systems;
using Squared.Tiled;
using System.Linq;

namespace Moonlight_Vale.Screens
{
    public class OverworldScreen : GameScreen
    {
        public IMap CurrentMap { get; private set; }
        public Player Player { get; private set; }
        public Camera2D Camera { get; private set; }
        public FontSystem FontSystem { get; private set; }
        public Desktop Desktop { get; private set; }

        public float Zoom { get; private set; } = 2.0f;

        public bool isInGameMenuActive;
        public bool isHUDActive;
        public bool isDevToolsActive;

        public KeyboardState previousKeyboardState { get; private set; }

        public HudManager HudManager { get; private set; }

        // Blokada klawisza E po przejściu
        private bool eKeyBlocked = false;

        public OverworldScreen(MoonlightVale game, ScreenManager manager, SpriteBatch batch, Desktop desktop)
            : base(game, manager, batch, desktop)
        {
            FontSystem = game._fontSystem;
            Desktop = desktop;
            isInGameMenuActive = false;
            isHUDActive = true;
            isDevToolsActive = false;
        }

        public override void Initialize()
        {
            TimeSystem.Instance.Start();
            Camera = new Camera2D();
            HudManager = new HudManager(this);
            HudManager.Initialize();
            previousKeyboardState = Keyboard.GetState();
        }

        public override void LoadContent(ContentManager content)
        {
            CurrentMap = new PlayerFarm(this);
            CurrentMap.LoadContent(content);

            Player = new Player(CurrentMap.PlayerSpawnPoint, CurrentMap);
            Player.LoadContent(content, @"Spritesheets\hero_spritesheet");
        }

        public void SaveGame() { }
        public void LoadGame() { }
        public void OpenSettings() { HudManager.CreateSettingsWindow(); }

        public void ReturnToMenu()
        {
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
            
            if (eKeyBlocked && !keyboard.IsKeyDown(Keys.E))
            {
                eKeyBlocked = false;
            }

            Camera.Zoom = Zoom;
            Player.Zoom = Zoom;
            Player.Speed = 120f;

            Player.Update(gameTime, keyboard);

            Camera.Position = Player.Position - new Vector2(1920 / 2f / Zoom - Player.SpriteWidth * Zoom / 2f,
                1080 / 2f / Zoom - Player.SpriteHeight * Zoom / 2f);

            if (keyboard.IsKeyDown(Keys.Escape) && previousKeyboardState.IsKeyUp(Keys.Escape))
                isInGameMenuActive = !isInGameMenuActive;

            if (keyboard.IsKeyDown(Keys.OemTilde) && previousKeyboardState.IsKeyUp(Keys.OemTilde))
                isDevToolsActive = !isDevToolsActive;

            if (keyboard.IsKeyDown(Keys.LeftAlt) && keyboard.IsKeyDown(Keys.Z) &&
                previousKeyboardState.IsKeyUp(Keys.Z))
                isHUDActive = !isHUDActive;

            HudManager.UpdateTime();
            HudManager.UpdateItemBarSelection(Player.SelectedItem);
            HudManager.UpdateVisibility(isHUDActive, isInGameMenuActive);
            
            if (!eKeyBlocked)
            {
                HandleMapTransitions(keyboard);
                
                // Check for PlayerHouse specific transitions
                if (CurrentMap is PlayerHouse playerHouse)
                {
                    if (CanUseEKey(keyboard))
                    {
                        playerHouse.CheckForExitTransition(keyboard, previousKeyboardState);
                    }
                }
            }
            
            previousKeyboardState = keyboard;
        }

        private bool CanUseEKey(KeyboardState keyboard)
        {
            return !eKeyBlocked && keyboard.IsKeyDown(Keys.E) && previousKeyboardState.IsKeyUp(Keys.E);
        }

        private void HandleMapTransitions(KeyboardState keyboard)
        {
            if (CurrentMap?.TileMap?.Layers == null || CurrentMap.TileMap.Layers.Count == 0)
                return;

            Vector2 tileIndex = GetTileIndex(Player.Position);
            var layer = CurrentMap.TileMap.Layers.Values.FirstOrDefault();
            
            if (layer == null || tileIndex.X < 0 || tileIndex.Y < 0 || 
                tileIndex.X >= layer.Width || tileIndex.Y >= layer.Height)
                return;

            int? tileId = layer.GetTile((int)tileIndex.X, (int)tileIndex.Y);
            
            if (CurrentMap is PlayerFarm && tileId == 83)
            {
                if (CanUseEKey(keyboard))
                {
                    SwitchToHouse();
                }
            }
            else if (CurrentMap is PlayerHouse && (tileId == 168 || tileId == 169))
            {
                if (CanUseEKey(keyboard))
                {
                    SwitchToFarm();
                }
            }
        }

        public void SwitchToHouse()
        {
            CurrentMap = new PlayerHouse(this);
            Player.Map = CurrentMap;
            CurrentMap.LoadContent(game.Content);
            Player.Position = CurrentMap.PlayerSpawnPoint;
            eKeyBlocked = true; // Zablokuj klawisz E
        }
        
        public void SwitchToFarm()
        {
            CurrentMap = new PlayerFarm(this);
            Player.Map = CurrentMap;
            CurrentMap.LoadContent(game.Content);
            Player.Position = new Vector2(550,520);
            eKeyBlocked = true; // Zablokuj klawisz E
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

            Player.Draw(spriteBatch);
            spriteBatch.End();

            if (isDevToolsActive)
            {
                DrawDebugInfo(spriteBatch);
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
                Vector2 tileIndex = GetTileIndex(Player.Position);
                font.DrawText(spriteBatch, $"Tile index: {tileIndex.X}, {tileIndex.Y}", new Vector2(20, 100), Color.White);

                var layer = CurrentMap.TileMap.Layers.Values.FirstOrDefault();
                if (layer != null && tileIndex.X >= 0 && tileIndex.Y >= 0 && 
                    tileIndex.X < layer.Width && tileIndex.Y < layer.Height)
                {
                    int? tileId = layer.GetTile((int)tileIndex.X, (int)tileIndex.Y);
                    font.DrawText(spriteBatch, $"Tile ID: {tileId}", new Vector2(20, 150), Color.White);
                }
            }

            font.DrawText(spriteBatch, $"Is HUD active: {isHUDActive}", new Vector2(20, 200), Color.White);
            font.DrawText(spriteBatch, $"Selected Item: {Player.SelectedItem}", new Vector2(20, 250), Color.White);
            font.DrawText(spriteBatch, $"Current Map: {CurrentMap?.GetType().Name}", new Vector2(20, 300), Color.White);
            font.DrawText(spriteBatch, $"UpBorder: {(int)Player.UpBorder}", new Vector2(20, 350), Color.White);
            font.DrawText(spriteBatch, $"DownBorder: {(int)Player.DownBorder}", new Vector2(20, 400), Color.White);
            font.DrawText(spriteBatch, $"LeftBorder: {(int)Player.LeftBorder}", new Vector2(20, 450), Color.White);
            font.DrawText(spriteBatch, $"RightBorder: {(int)Player.RightBorder}", new Vector2(20, 500), Color.White);

            DrawPlayerHitbox(spriteBatch);
            DrawTargetTile(spriteBatch);
            
            spriteBatch.End();
        }

        private void DrawPlayerHitbox(SpriteBatch spriteBatch)
        {
            var _texture = new Texture2D(graphicsDevice, 1, 1);
            _texture.SetData(new Color[] { new (100, 250, 100, 75) });

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
            Vector2 screenPosition = Vector2.Transform(tileWorldPos, Camera.GetViewMatrix());
            
            var whiteTexture = new Texture2D(graphicsDevice, 1, 1);
            whiteTexture.SetData(new[] { new Color(200, 200, 200, 5) });
            
            int scaledSize = (int)(tileSize * Zoom * 2);
            spriteBatch.Draw(whiteTexture, new Rectangle((int)screenPosition.X, (int)screenPosition.Y, scaledSize, scaledSize), Color.White);
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
            Desktop.Widgets.Clear();
            Desktop.Root = null;
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