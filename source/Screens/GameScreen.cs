using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;
using Myra.Graphics2D.UI;

namespace Moonlight_Vale.Screens
{
    public abstract class GameScreen 
    {
        public MoonlightVale game;
        protected ScreenManager screenManager;
        protected ContentManager content;
        protected GraphicsDevice graphicsDevice;
        protected FontStashSharp.FontSystem fontSystem;
        protected Desktop desktop;
        protected SpriteBatch spriteBatch;
        protected Song soundTrack;
        protected bool isSongPlaying;

        public GameScreen(MoonlightVale game, ScreenManager screenManager, SpriteBatch spriteBatch, Desktop desktop)
        {
            this.game = game;
            this.screenManager = screenManager;
            this.content = game.Content;
            this.graphicsDevice = game.GraphicsDevice;
            this.desktop = desktop;
            this.spriteBatch = spriteBatch;
        }

        public abstract void Initialize();

        public abstract void LoadContent(ContentManager content);

        public abstract void Update(GameTime gameTime);

        public abstract void Draw(GameTime gameTime, SpriteBatch spriteBatch);
        public abstract void Unload();
    }
}