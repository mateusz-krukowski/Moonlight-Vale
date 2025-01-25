using System;
using FontStashSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Myra.Graphics2D;
using Myra.Graphics2D.Brushes;
using Myra.Graphics2D.UI;

namespace Moonlight_Vale.Screens;

public class SplashScreen : GameScreen
{
    
    private double _elapsedTime; 
    private const double _displayDuration = 3000;
    
    public SplashScreen(MoonlightVale game, ScreenManager screenManager, SpriteBatch spriteBatch, Desktop desktop, FontSystem fontSystem) : base(game, screenManager,
        spriteBatch, desktop)
    {
        this.game = game;
        this.screenManager = screenManager;
        this.spriteBatch = spriteBatch;
        this.fontSystem = fontSystem;
        this.desktop = desktop;
        
    }

    public override void Initialize()
    {
        
        
        var panel = new Panel();

        var titleLabel = new Label
        {
            Text = "Welcome to\nMoonlight Vale",
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Font = fontSystem.GetFont(6),
            TextColor = Color.White,
            VerticalSpacing = 2,
            Margin = new Thickness(0, 0, 0, 0),
            Background = new SolidBrush(Color.Transparent)
        };
        
        panel.Widgets.Add(titleLabel);
        
        desktop.Root = panel;
    }

    public override void LoadContent(ContentManager content)
    {
        
    }

    public override void Update(GameTime gameTime)
    {
        _elapsedTime += gameTime.ElapsedGameTime.TotalMilliseconds;

        
        if (_elapsedTime >= _displayDuration)
        {
            Dispose(); 
        }
    }

    public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
    {
        graphicsDevice.Clear(new Color(10,10,10));
        desktop.Render();
    }

    public override void Unload()
    {
        desktop.Widgets.Clear();
    }
    
    public void Dispose()
    {
        // Wywołaj funkcję usuwającą ekran z maszyny stanów
        screenManager.RemoveScreen();
        screenManager.AddScreen(new MainMenuScreen(game, screenManager, spriteBatch, desktop, fontSystem));

    }
}