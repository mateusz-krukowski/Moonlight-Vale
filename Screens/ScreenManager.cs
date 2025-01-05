using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace Moonlight_Vale.Screens;

public class ScreenManager
{
    private static ScreenManager _instance;
    private Game _game;
    private Stack<GameScreen> _screens = new Stack<GameScreen>();
    private ContentManager _content;


    private ScreenManager(Game game)
    {
        _game = game;
        _content = game.Content;
    }

    public static ScreenManager Instance
    {
        get
        {
            if (_instance == null)
            {
                throw new System.Exception("ScreenManager is not initialized.");
            }
            return _instance;
        }
    }

    public static void Initialize(Game game)
    {
        if( _instance == null)
        {
            _instance = new ScreenManager(game); 
        }
    }

    public void InitializeScreen()
    {
        if (_screens.Count > 0)
            _screens.Peek().Initialize();
    }

    public void LoadContent()
    {
        if (_screens.Count > 0)
            _screens.Peek().LoadContent(_content);
    }

    public void Update(GameTime gameTime)
    {
        if (_screens.Count > 0)
            _screens.Peek().Update(gameTime);
    }

    public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
    {
        if (_screens.Count > 0)
            _screens.Peek().Draw(gameTime, spriteBatch);
            
    }

    public void AddScreen(GameScreen screen)
    {
        _screens.Push(screen);
        screen.Initialize();
        screen.LoadContent(_content);
    }

    public void RemoveScreen()
    {
        if (_screens.Count >= 0)
            _screens.Pop().Unload();
    }
}