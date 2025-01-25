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

namespace Moonlight_Vale.Screens;

public class OverworldScreen : GameScreen
{
    private Map map;
    private Texture2D tileSet;
    
    private float zoom = 4.0f;
    private int prevScroll;
    private const float MinZoom = 0.5f, MaxZoom = 10.0f, ZoomSpeed = 0.001f;
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
        CreateInGameMenu();
        previousKeyboardState = Keyboard.GetState(); // Initialize previous keyboard state
    }

    public override void LoadContent(ContentManager content)
    {
        map = Map.Load(content.RootDirectory + @"\Tilemaps\player_farm_reduced.tmx", content);
        tileSet = map.Tilesets.Values.First().Texture;
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
        game.Exit();
    }

    public override void Update(GameTime gameTime)
    {
        var keyboard = Keyboard.GetState();
        var mouse = Mouse.GetState();

        // Handle zooming with the mouse
        if (keyboard.IsKeyDown(Keys.LeftControl))
        {
            zoom = Math.Clamp(zoom + (mouse.ScrollWheelValue - prevScroll) * ZoomSpeed, MinZoom, MaxZoom);
            prevScroll = mouse.ScrollWheelValue;
            
        }

        // Handle toggling the in-game menu
        if (keyboard.IsKeyDown(Keys.Escape) && previousKeyboardState.IsKeyUp(Keys.Escape))
        {
            isInGameMenuActive = !isInGameMenuActive;

        }

        previousKeyboardState = keyboard; // Update the previous keyboard state
    }

    public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
    {
        graphicsDevice.Clear(Color.Aqua);
        if (map == null) return;

        spriteBatch.Begin(samplerState: SamplerState.PointClamp);

        foreach (var layer in map.Layers.Values)
        {
            DrawLayer(spriteBatch, layer);
        }

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

