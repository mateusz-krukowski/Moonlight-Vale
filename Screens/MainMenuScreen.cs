using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;
using Myra.Graphics2D;
using Myra.Graphics2D.UI;
using Myra.Graphics2D.Brushes;

namespace Moonlight_Vale.Screens;

public class MainMenuScreen : GameScreen
{
    public MainMenuScreen(Game game, ScreenManager screenManager, SpriteBatch spriteBatch, Desktop desktop, FontStashSharp.SpriteFontBase font) :
        base(game, screenManager, spriteBatch, desktop)
    {
        this.game = game;
        this.spriteBatch = spriteBatch;
        this.desktop = desktop;
        this.font = font;
        
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
            Font = font,
            TextColor = Color.White,
            VerticalSpacing = 2,
            Margin = new Thickness(0, 100, 0, 0),
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
        
    }

    public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
    {
        graphicsDevice.Clear(new Color(250, 128, 114));
        desktop.Render(); // !!! important to make GUI visible
    }

    public override void Unload()
    {
        
    }
}
