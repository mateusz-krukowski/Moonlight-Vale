using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Squared.Tiled;
using System.Linq;
using FontStashSharp;
using Myra.Graphics2D.UI;
using Moonlight_Vale.Entity;

namespace Moonlight_Vale.Screens
{
    public class OverworldScreen : GameScreen
    {
        public Map Map { get; private set; }
        public Texture2D TileSet { get; private set; }
        public Player Player { get; private set; }
        public Camera2D Camera { get; private set; }
        public float Zoom { get; private set; } = 2.0f;
        public FontSystem FontSystem { get; private set; }
        public Desktop Desktop { get; private set; }

        public bool isInGameMenuActive;
        public bool isHUDActive;
        public bool isDevToolsActive;

        public KeyboardState previousKeyboardState { get; private set; }

        public HudManager HudManager { get; private set; }

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
            Camera = new Camera2D();

            HudManager = new HudManager(this);
            HudManager.Initialize();

            previousKeyboardState = Keyboard.GetState();
        }

        public override void LoadContent(ContentManager content)
        {
            Map = Map.Load(content.RootDirectory + @"\Tilemaps\player_farm_reduced.tmx", content);
            TileSet = Map.Tilesets.Values.First().Texture;
            Player = new Player(new Vector2(129, 150), Map);
            Player.LoadContent(content, @"Spritesheets\hero_spritesheet");
        }

        public void SaveGame() { }
        public void LoadGame() { }
        public void OpenSettings() { }

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
            var mouse = Mouse.GetState();

            Player.Update(gameTime, keyboard);

            Camera.Zoom = Zoom;
            Player.Zoom = Zoom;
            Player.Speed = Player.Zoom * 50.0f;

            Camera.Position = Player.Position - new Vector2(1920 / 2f / Zoom - Player.SpriteWidth * Zoom / 2f,
                                                            1080 / 2f / Zoom - Player.SpriteHeight * Zoom / 2f);

            if (keyboard.IsKeyDown(Keys.Escape) && previousKeyboardState.IsKeyUp(Keys.Escape))
                isInGameMenuActive = !isInGameMenuActive;

            if (keyboard.IsKeyDown(Keys.OemTilde) && previousKeyboardState.IsKeyUp(Keys.OemTilde))
                isDevToolsActive = !isDevToolsActive;

            if (keyboard.IsKeyDown(Keys.LeftAlt) && keyboard.IsKeyDown(Keys.Z) &&
                previousKeyboardState.IsKeyUp(Keys.Z))
                isHUDActive = !isHUDActive;

            HudManager.UpdateVisibility(isHUDActive, isInGameMenuActive);

            previousKeyboardState = keyboard;
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            graphicsDevice.Clear(Color.Black);
            if (Map == null) return;

            spriteBatch.Begin(transformMatrix: Camera.GetViewMatrix(), samplerState: SamplerState.PointClamp);

            foreach (var layer in Map.Layers.Values)
            {
                DrawLayer(spriteBatch, layer);
            }

            Player.Draw(spriteBatch);
            spriteBatch.End();

            if (isDevToolsActive)
            {
                spriteBatch.Begin();
                var font = FontSystem.GetFont(4);

                font.DrawText(spriteBatch, $"Player position: {(int)Player.Position.X}, {(int)Player.Position.Y}", new Vector2(20, 50), Color.White);

                Vector2? tileIndex = GetTileIndex(Player.Position);
                font.DrawText(spriteBatch, $"Tile index: {tileIndex?.X}, {tileIndex?.Y}", new Vector2(20, 100), Color.White);

                var layer = Map.Layers.Values.First();
                int? tileId = layer.GetTile((int)tileIndex?.X, (int)tileIndex?.Y);
                font.DrawText(spriteBatch, $"Tile ID: {tileId}", new Vector2(20, 150), Color.White);

                font.DrawText(spriteBatch, $"Is HUD active: {isHUDActive}", new Vector2(20, 200), Color.White);
                font.DrawText(spriteBatch, $"Selected Item: {Player.SelectedItem}", new Vector2(20, 250), Color.White);

                font.DrawText(spriteBatch, $"UpBorder: {(int)Player.UpBorder}", new Vector2(20, 300), Color.White);
                font.DrawText(spriteBatch, $"DownBorder: {(int)Player.DownBorder}", new Vector2(20, 350), Color.White);
                font.DrawText(spriteBatch, $"LeftBorder: {(int)Player.LeftBorder}", new Vector2(20, 400), Color.White);
                font.DrawText(spriteBatch, $"RightBorder: {(int)Player.RightBorder}", new Vector2(20, 450), Color.White);

                spriteBatch.End();
            }

            HudManager.Draw();
        }

        private void DrawLayer(SpriteBatch spriteBatch, Layer layer)
        {
            int scaledTileSize = (int)(Map.TileWidth * Zoom);

            for (int y = 0; y < layer.Height; y++)
            for (int x = 0; x < layer.Width; x++)
            {
                int tileIndex = layer.GetTile(x, y);

                var tileRect = new Rectangle();
                if (!Map.Tilesets.First().Value.MapTileToRect(tileIndex, ref tileRect)) continue;

                spriteBatch.Draw(TileSet, new Vector2(x, y) * scaledTileSize, tileRect, Color.White, 0, Vector2.Zero, Zoom, SpriteEffects.None, 0);
            }
        }

        public override void Unload()
        {
            Desktop.Widgets.Clear();
            Desktop.Root = null;
        }

        private Vector2 GetTileIndex(Vector2 position)
        {
            int tileX = (int)(position.X / (Map.TileWidth * Zoom));
            int tileY = (int)(position.Y / (Map.TileHeight * Zoom));
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
