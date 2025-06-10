using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;
using Moonlight_Vale.Systems;
using Myra.Graphics2D;
using Myra.Graphics2D.UI;
using Myra.Graphics2D.Brushes;

namespace Moonlight_Vale.Screens
{
    public class MainMenuScreen : GameScreen
    {
        private Song mainMenuSong;
        private Texture2D backgroundTexture;
    
        /*colors*/
        private static readonly Color BUTTON_COLOR = new Color(129,88,46); //base
        private static readonly Color BUTTON_HOVER_COLOR = new Color(142,105,67); //lighter
        private static readonly Color BUTTON_PRESSED_COLOR = new Color(116,79,41); //darker
    
        public SavingSystem SavingSystem { get; private set; }
    
        public MainMenuScreen(MoonlightVale game, ScreenManager screenManager, SpriteBatch spriteBatch, Desktop desktop, FontStashSharp.FontSystem fontSystem) :
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
                Left = 1420,
                Top = 160,
                Spacing = 38 
            };
        
            stackPanel.Widgets.Add(CreateButton("New Game"));
            stackPanel.Widgets.Add(CreateButton("Continue"));
            stackPanel.Widgets.Add(CreateButton("Settings"));
            stackPanel.Widgets.Add(CreateButton("Credits"));
            stackPanel.Widgets.Add(CreateButton("Exit"));
        
            panel.Widgets.Add(titleLabel);
            panel.Widgets.Add(stackPanel);

            desktop.Root = panel;
        }

        public override void LoadContent(ContentManager content)
        {
            mainMenuSong = game.Content.Load<Song>( @"Music\MainTheme" );
            backgroundTexture = content.Load<Texture2D>(@"Images\Main_Menu");
            MediaPlayer.Play(mainMenuSong);
            MediaPlayer.Volume = 0.5f;
            MediaPlayer.IsRepeating = true; 
        }

        public override void Update(GameTime gameTime)
        {
 
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            graphicsDevice.Clear(new Color(135, 206, 235));
            spriteBatch.Begin();
            spriteBatch.Draw(backgroundTexture, new Rectangle(0, 0, graphicsDevice.Viewport.Width, graphicsDevice.Viewport.Height), Color.White);
            spriteBatch.End();
            desktop.Render(); // !!! important to make GUI visible
        
        }

        public override void Unload()
        {
            MediaPlayer.Stop();
            mainMenuSong = null;
            desktop.Widgets.Clear();
            desktop.Root = null;

        }
    
        TextButton CreateButton(string text)
        {
            var button = new TextButton
            {
                Text = text,
                Font = fontSystem.GetFont(3.5f),
                Padding = new Thickness(0,35,0,0),
            
                TextColor = Color.White,
                PressedTextColor = Color.LightGray,
            
                Background = new SolidBrush(BUTTON_COLOR),
                PressedBackground = new SolidBrush(BUTTON_PRESSED_COLOR),
                OverBackground = new SolidBrush(BUTTON_HOVER_COLOR),
                FocusedBackground = new SolidBrush(Color.Transparent),
            
                BorderThickness = new Thickness(2),
                Border = new SolidBrush(Color.Black),
            
                Width = 370, //TODO: proportional to the number of letters!
                Height = 85,
          
            };
        
        
            button.Click += (s, e) =>
            {
            
                switch (button.Text)
                {
                    case "New Game":
                    {
                        screenManager.RemoveScreen();
                        screenManager.AddScreen(new OverworldScreen(game,screenManager,spriteBatch,desktop));
                        break;
                    }
                    case "Exit":
                    {
                        Unload();
                        game.Exit();
                        break;
                    }
                }
            };
 

            return button;
        }
    
    }
}
