using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Squared.Tiled;
using System.Linq;
using FontStashSharp;
using FontStashSharp.RichText;
using Myra.Graphics2D;
using Myra.Graphics2D.Brushes;
using Myra.Graphics2D.UI;
using Moonlight_Vale.Entity;

namespace Moonlight_Vale.Screens;

public class OverworldScreen : GameScreen
{
    private Map map;
    private Texture2D tileSet;
    private Player player;
    private Camera2D camera;

    private float zoom = 2.0f;
    private int prevScroll;
    private const float MinZoom = 1.5f, MaxZoom = 5.0f, ZoomSpeed = 0.1f;

    private FontSystem fontSystem;
    private Desktop desktop;

    public VerticalStackPanel InGameMenu { get; private set; }

    public Grid HUD { get; private set; }
    
    
//! DevTools are being displayed immediately via FontStashSharp not MyraUI!

    private bool isInGameMenuActive;
    private bool isHUDActive;
    private bool isDevToolsActive;

    private KeyboardState previousKeyboardState;

    public OverworldScreen(MoonlightVale game, ScreenManager manager, SpriteBatch batch, Desktop desktop) 
        : base(game, manager, batch, desktop)
    {
        this.fontSystem = game._fontSystem;
        this.desktop = desktop;
        isInGameMenuActive = false;
        isHUDActive = true;
        isDevToolsActive = false;
    }

    public override void Initialize()
    {
        player = new Player(new Vector2(129, 90));
        camera = new Camera2D();

        // Create the root panel and sub-panels
        var rootPanel = new Panel();

        InGameMenu = CreateInGameMenu();

        HUD = CreateHUD();

        rootPanel.Widgets.Add(HUD);
        rootPanel.Widgets.Add(InGameMenu);

        desktop.Root = rootPanel;

        previousKeyboardState = Keyboard.GetState();
    }

    public override void LoadContent(ContentManager content)
    {
        map = Map.Load(content.RootDirectory + @"\Tilemaps\player_farm_reduced.tmx", content);
        tileSet = map.Tilesets.Values.First().Texture;
        player.LoadContent(content, @"Spritesheets\hero_spritesheet");
    }

    private VerticalStackPanel CreateInGameMenu()
    {
        var menu = new VerticalStackPanel
        {
            Width = 1920,
            Height = 1080,
            Spacing = 20,
            Margin = new Thickness(0, 360, 0, 0),
            
        };

        menu.Widgets.Add(CreateButton("Return to Game", () => { isInGameMenuActive = false; }));
        menu.Widgets.Add(CreateButton("Save Game", SaveGame));
        menu.Widgets.Add(CreateButton("Load Game", LoadGame));
        menu.Widgets.Add(CreateButton("Settings", OpenSettings));
        menu.Widgets.Add(CreateButton("Return to Menu", ReturnToMenu));
        menu.Widgets.Add(CreateButton("Exit Game", ExitGame));
        menu.Background = new SolidBrush(Color.Transparent);

        menu.Visible = false;
        return menu;
    }
    

    private Grid CreateHUD()
    {
        // Tworzenie głównego kontenera HUD
        var hud = new Grid
        {
            Background = new SolidBrush(Color.Transparent),
            ShowGridLines = true,
            Visible = true,
            Width = 1920,
            Height = 1080
        };

        // Tworzenie ItemBarStackPanel (10 kwadratów)
        var itemBarStackPanel = new HorizontalStackPanel
        {
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Bottom,
            Padding = new Thickness(0, 0, 0, 64),
            Spacing = 8 // Odstęp między kwadratami
        };

        for (int i = 0; i < 10; i++)
        {
            var square = new Panel
            {
                Width = 64,
                Height = 64,
                Background = new SolidBrush(Color.LightGray),
                Border = new SolidBrush(Color.White),
                BorderThickness = new Thickness(4)
            };
            itemBarStackPanel.Widgets.Add(square);
        }

        // Tworzenie UtilitiesStackPanel (prostokąty z literami)
        var utilitiesStackPanel = new HorizontalStackPanel
        {
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Bottom,
            Padding = new Thickness(0, 0, 100, 64),
            Spacing = 8 // Odstęp między prostokątami
        };

        string[] letters = { "c", "b", "j", "m" };
        foreach (var letter in letters)
        {
            var rectangle = new Label
            {
                Width = 60,
                Height = 60,
                Text = letter,
                Font = fontSystem.GetFont(4),
                TextColor = Color.White,
                TextAlign = TextHorizontalAlignment.Center,
                
                Padding = new Thickness(0, 24, 0, 0),
                
                Background = new SolidBrush(Color.LightGray),
                Border = new SolidBrush(Color.White),
                BorderThickness = new Thickness(1),
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center
            };
            utilitiesStackPanel.Widgets.Add(rectangle);
        }

        // Dodanie obu paneli do HUD
        hud.Widgets.Add(itemBarStackPanel);
        hud.Widgets.Add(utilitiesStackPanel);

        return hud;
    }
    
    private void SaveGame() { }
    private void LoadGame() { }
    private void OpenSettings() { }

    private void ReturnToMenu()
    {
        screenManager.RemoveScreen();
        screenManager.AddScreen(new MainMenuScreen(game, screenManager, spriteBatch, desktop, fontSystem));
    }

    private void ExitGame()
    {
        screenManager.RemoveScreen();
        game.Exit();
    }

    public override void Update(GameTime gameTime)
    {
        var keyboard = Keyboard.GetState();
        var mouse = Mouse.GetState();

        player.Update(gameTime, keyboard);

        // Zoom logic
        if (mouse.ScrollWheelValue > prevScroll)
        {
            zoom = Math.Min(zoom + ZoomSpeed, MaxZoom);
        }
        else if (mouse.ScrollWheelValue < prevScroll)
        {
            zoom = Math.Max(zoom - ZoomSpeed, MinZoom);
        }
        prevScroll = mouse.ScrollWheelValue;

        camera.Zoom = zoom;
        player.Zoom = zoom;
        player.Speed = player.Zoom * 50.0f;

        camera.Position = player.Position - new Vector2(1920 / 2f / zoom - player.SpriteWidth * zoom / 2f,
                                                        1080 / 2f / zoom - player.SpriteHeight * zoom / 2f);

        // Toggle panels
        if (keyboard.IsKeyDown(Keys.Escape) && previousKeyboardState.IsKeyUp(Keys.Escape))
        {
            isInGameMenuActive = !isInGameMenuActive;
        }

        if (keyboard.IsKeyDown(Keys.OemTilde) && previousKeyboardState.IsKeyUp(Keys.OemTilde))
        {
            isDevToolsActive = !isDevToolsActive;
        }

        if (keyboard.IsKeyDown(Keys.LeftAlt) && keyboard.IsKeyDown(Keys.Z) &&
            previousKeyboardState.IsKeyUp(Keys.Z))
        {
            isHUDActive = !isHUDActive;
        }

        // Update panel visibility
        HUD.Visible = isHUDActive;
        InGameMenu.Visible = isInGameMenuActive;

        previousKeyboardState = keyboard;
    }

    public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
    {
        graphicsDevice.Clear(Color.Black);
        if (map == null) return;

        spriteBatch.Begin(transformMatrix: camera.GetViewMatrix(), samplerState: SamplerState.PointClamp);

        foreach (var layer in map.Layers.Values)
        {
            DrawLayer(spriteBatch, layer);
        }

        player.Draw(spriteBatch);
        spriteBatch.End();

        if (isDevToolsActive)
        {
            spriteBatch.Begin();
            var font = fontSystem.GetFont(4);
            
            font.DrawText(spriteBatch, $"Player position: {(int)player.Position.X}, {(int)player.Position.Y}", new Vector2(20,50), Color.White);
            
            Vector2? tileIndex = GetTileIndex(player.Position);
            font.DrawText(spriteBatch, $"Tile index: {tileIndex?.X}, {tileIndex?.Y}", new Vector2(20, 100), Color.White);
            
            var layer = map.Layers.Values.First(); 
            int? tileId = layer.GetTile((int)tileIndex?.X, (int)tileIndex?.Y);
            font.DrawText(spriteBatch, $"Tile ID: {tileId}", new Vector2(20, 150), Color.White);
            
            font.DrawText(spriteBatch, $"Is HUD active: {isHUDActive}", new Vector2(20, 200), Color.White);
            font.DrawText(spriteBatch, $"Selected Item: {player.SelectedItem}", new Vector2(20, 250), Color.White);
            
            spriteBatch.End();
        }
        
        // Render all panels via desktop
        desktop.Render();
    }

    private void DrawLayer(SpriteBatch spriteBatch, Layer layer)
    {
        int scaledTileSize = (int)(map.TileWidth * zoom);

        for (int y = 0; y < layer.Height; y++)
        for (int x = 0; x < layer.Width; x++)
        {
            int tileIndex = layer.GetTile(x, y);

            var tileRect = new Rectangle();
            if (!map.Tilesets.First().Value.MapTileToRect(tileIndex, ref tileRect)) continue;

            spriteBatch.Draw(tileSet, new Vector2(x, y) * scaledTileSize, tileRect, Color.White, 0, Vector2.Zero, zoom, SpriteEffects.None, 0);
        }
    }

    public override void Unload()
    {
        desktop.Widgets.Clear();
        desktop.Root = null;
    }

    private TextButton CreateButton(string text, Action onClick)
    {
        var button = new TextButton
        {
            Text = text,
            Font = fontSystem.GetFont(2.3f),
            Width = 220,
            Height = 45,
            Padding = new Thickness(0, 20, 0, 0),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };

        button.Click += (s, e) => onClick?.Invoke();

        return button;
    }
    
    private Vector2 GetTileIndex(Vector2 position)
    {
        int tileX = (int)(position.X / (map.TileWidth * zoom));
        int tileY = (int)(position.Y / (map.TileHeight * zoom));
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