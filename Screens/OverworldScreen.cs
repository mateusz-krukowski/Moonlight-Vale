using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Squared.Tiled;
using System.Linq;
using FontStashSharp;
using Myra.Graphics2D;
using Myra.Graphics2D.Brushes;
using Myra.Graphics2D.UI;
using MoonlightVale.Player;

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
    private bool isInGameMenuActive;

    private FontSystem fontSystem;
    private Desktop desktop;
    private Panel inGameMenu;

    private KeyboardState previousKeyboardState;

    public OverworldScreen(MoonlightVale game, ScreenManager manager, SpriteBatch batch, Desktop desktop) 
        : base(game, manager, batch, desktop)
    {
        this.fontSystem = game._fontSystem;
        this.desktop = desktop;
        isInGameMenuActive = false;
    }

    public override void Initialize()
    {
        player = new Player(new Vector2(129,90));
        camera = new Camera2D(); // Inicjalizacja kamery
        CreateInGameMenu();
        previousKeyboardState = Keyboard.GetState(); // Initialize previous keyboard state
    }

    public override void LoadContent(ContentManager content)
    {
        map = Map.Load(content.RootDirectory + @"\Tilemaps\player_farm_reduced.tmx", content);
        tileSet = map.Tilesets.Values.First().Texture;
        player.LoadContent(content, @"Spritesheets\hero_spritesheet");
    }
    
    private void CreateInGameMenu()
    {
        var menu = new VerticalStackPanel
        {
            
            Width = 1920,
            Height = 1080,
            Spacing = 20,
            Margin = new Thickness(0,200,0,0),
            
            
        };
        
        menu.Widgets.Add(createButton("Return to Game", () => {isInGameMenuActive = !isInGameMenuActive;}));
        menu.Widgets.Add(createButton("Save Game", saveGame));
        menu.Widgets.Add(createButton("Load Game", loadGame));
        menu.Widgets.Add(createButton("Settings", openSettings));
        menu.Widgets.Add(createButton("Return to Menu", returnToMenu));
        menu.Widgets.Add(createButton("Exit Game", exitGame));
        menu.Background = new SolidBrush(Color.Transparent);

        var panel = new Panel();
        panel.Widgets.Add(menu);
        desktop.Root = panel;
    }

    private void saveGame()
    {
        
    }

    private void loadGame()
    {
        
    }

    private void openSettings()
    {
        
    }

    private void returnToMenu()
    {
        screenManager.RemoveScreen();
        screenManager.AddScreen(new MainMenuScreen(game,screenManager,spriteBatch,desktop, fontSystem));
    }

    private void exitGame()
    {
        screenManager.RemoveScreen();
        game.Exit();
    }

    public override void Update(GameTime gameTime)
    {
        var keyboard = Keyboard.GetState();
        var mouse = Mouse.GetState();

        player.Update(gameTime, keyboard);

        // Obsługa zoomu za pomocą scrolla myszki
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
        player.Speed = player.Zoom * 50.0f; //to prevent higher speed on smaller zoom value

        // Ustawienie kamery tak, aby bohater był na środku ekranu
        camera.Position = player.Position - new Vector2( x: 1920 / 2f / zoom - Player.SpriteWidth * zoom / 2f,
                                                         y: 1080 / 2f / zoom - Player.SpriteHeight * zoom / 2f);

        // Obsługa menu gry
        if (keyboard.IsKeyDown(Keys.Escape) && previousKeyboardState.IsKeyUp(Keys.Escape))
        {
            isInGameMenuActive = !isInGameMenuActive;
        }

        previousKeyboardState = keyboard; // Aktualizacja poprzedniego stanu klawiatury
    }

    public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
    {
        graphicsDevice.Clear(Color.Aqua);
        if (map == null) return;

        
        spriteBatch.Begin(transformMatrix: camera.GetViewMatrix(), samplerState: SamplerState.PointClamp);

        foreach (var layer in map.Layers.Values)
        {
            DrawLayer(spriteBatch, layer);
        }
        
        player.Draw(spriteBatch);
        spriteBatch.End();
        
        if (isInGameMenuActive)
        {
            spriteBatch.Begin();
            var font = fontSystem.GetFont(12);
            font.DrawText(spriteBatch, "IMGUI layer test text lorem ipsum", new Vector2(200,200), Color.White);
            spriteBatch.End();
            
            desktop.Render();
        }
        
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

    private TextButton createButton(String text, Action onClick)
    {
        var button = new TextButton
        {
            Text = text,
            Font = fontSystem.GetFont(2.2f),
            Width = 200,
            Height = 40,
            Padding = new Thickness(0,20,0,0),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };
        
        button.Click += (s,e) => onClick?.Invoke();
        
        return button;
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