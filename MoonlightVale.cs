using System.Diagnostics.CodeAnalysis;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Myra;
using Myra.Graphics2D.UI;
using FontStashSharp;
using Microsoft.Xna.Framework.Media;
using Moonlight_Vale.Screens;

namespace Moonlight_Vale
{
    public class MoonlightVale : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private Desktop _desktop;
        private ScreenManager _screenManager;
        private FontSystem _fontSystem;
        
        private const byte WIDTH_RATIO = 16 , HEIGHT_RATIO = 9;

        public MoonlightVale()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {
            base.Initialize();
            ScreenManager.Initialize(this);

            Window.Title = "Moonlight Vale";
            _graphics.PreferredBackBufferWidth = 1920;
            _graphics.PreferredBackBufferHeight = 1080;
            _graphics.ToggleFullScreen();

            MyraEnvironment.Game = this;
            _desktop = new Desktop();

            _screenManager = ScreenManager.Instance;
            _screenManager.AddScreen(new SplashScreen(this, _screenManager, _spriteBatch, _desktop, _fontSystem));
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _fontSystem = new FontSystem();
            _fontSystem.AddFont(System.IO.File.ReadAllBytes(Content.RootDirectory + @"\Fonts\CelticBitRegular.ttf"));
        }

        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            _screenManager.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            _screenManager.Draw(gameTime, _spriteBatch);
            base.Draw(gameTime);
        }
    }

}