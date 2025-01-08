using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Myra.Graphics2D;
using Myra.Graphics2D.UI;
using Myra.Graphics2D.Brushes;

namespace Moonlight_Vale.Screens;

public class MainMenuScreen : GameScreen
{
    public MainMenuScreen(Game game, ScreenManager screenManager, SpriteBatch spriteBatch, Desktop desktop, FontStashSharp.FontSystem fontSystem) :
        base(game, screenManager, spriteBatch, desktop)
    {
        this.game = game;
        this.screenManager = screenManager;
        this.spriteBatch = spriteBatch;
        this.desktop = desktop;
        this.fontSystem = fontSystem;
        
    }

    public override void Initialize()
    {
        /*TODO add: soundTrack = game.Content.Load<Song>("path_to_file") */
        
        game.IsMouseVisible = true;
        var panel = new Panel();
        
        var titleLabel = new Label
        {
            Text = "Moonlight Vale",
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Top,
            Font = fontSystem.GetFont(8),
            TextColor = Color.White,
            VerticalSpacing = 2,
            Margin = new Thickness(0, 100, 0, 0),
            Background = new SolidBrush(Color.Transparent)
        };
        
        var stackPanel = new VerticalStackPanel
        {
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
            Left = 1480,
            Top = 280,
            Spacing = 20 // Odstępy między przyciskami
        };
        
        stackPanel.Widgets.Add(CreateButton("New Game"));
        stackPanel.Widgets.Add(CreateButton("Continue"));
        stackPanel.Widgets.Add(CreateButton("Settings"));
        stackPanel.Widgets.Add(CreateButton("Credits"));
        stackPanel.Widgets.Add(CreateButton("Exit"));
        
        panel.Widgets.Add(titleLabel);
        panel.Widgets.Add(stackPanel);
        foreach (var widget in panel.Widgets)
        {
            Console.WriteLine(widget);
        }
       
 
        // Dodanie tabeli do Desktop

        desktop.Root = panel;
        
        
    }

    public override void LoadContent(ContentManager content)
    {
        
    }

    public override void Update(GameTime gameTime)
    {
        
    }

    public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
    {
        graphicsDevice.Clear(new Color(250, 128, 114));
        desktop.Render(); // !!! important to make GUI visible
        
    }
    

    public override void Unload()
    {
        
    }
    
    TextButton CreateButton(string text)
    {
        var button = new TextButton
        {
            Text = text,
            Background = new SolidBrush(Color.FromNonPremultiplied(249, 246, 230, 255)),
            TextColor = Color.Black,
            Font = fontSystem.GetFont(3),

            ContentHorizontalAlignment = HorizontalAlignment.Center,
            ContentVerticalAlignment = VerticalAlignment.Center,
            Width = 300,
            Height = 50,
            ClipToBounds = false // Aby zaokrąglenie działało poprawnie
        };

   
        

        // Efekt najechania
        button.MouseEntered += (s, e) =>
        {
            button.Background = new SolidBrush(Color.Black);
            button.TextColor = Color.White;
        };
        button.MouseLeft += (s, e) =>
        {
            button.Background = new SolidBrush(new Color(249, 246, 230, 255));
            button.TextColor = Color.Black;
        };

        button.Click += (s, e) =>
        {
            switch (button.Text)
            {
                case "New Game": screenManager.AddScreen(new OverworldScreen(game,screenManager,spriteBatch,desktop)); break;
                case "Exit": game.Exit(); break;
            }
        };
 

        return button;
    }
    
}
