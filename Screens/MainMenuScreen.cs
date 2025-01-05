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
        var panel = new Panel();
        /*TODO add: soundTrack = game.Content.Load<Song>("path_to_file") */
        
        game.IsMouseVisible = true;
        
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
    }

    public override void Unload()
    {
        
    }
}
