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
    private readonly double _displayDuration = 2500;
    
    public SplashScreen(Game game, ScreenManager screenManager, SpriteBatch spriteBatch, Desktop desktop, SpriteFontBase font) : base(game, screenManager,
        spriteBatch, desktop)
    {
        this.game = game;
        this.screenManager = screenManager;
        this.spriteBatch = spriteBatch;
        this.font = font;
        this.desktop = desktop;
        
    }

    public override void Initialize()
    {
        
        
        var panel = new Panel();

        var titleLabel = new Label
        {
            Text = "Welcome to\n Moonlight Vale Game",
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Font = font,
            TextColor = Color.White,
            Margin = new Thickness(0, 150, 0, 0),
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
    }
}