using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Myra.Graphics2D.UI;
using Squared.Tiled;
using System.Linq;
using Microsoft.Xna.Framework.Input;

namespace Moonlight_Vale.Screens;

public class OverworldScreen : GameScreen
{
    Map map;
    Texture2D tileSet;
    private float _zoomLevel = 4.0f; // Domyślna wartość powiększenia
    private int _previousScrollValue; // Poprzedni stan scrolla
    private const float MinZoom = 0.5f; // Minimalny zoom
    private const float MaxZoom = 10.0f; // Maksymalny zoom
    private const float zoomSpeed = 0.001f; // Szybkość zoomowania
    
    public OverworldScreen(Game game, ScreenManager screenManager, SpriteBatch spriteBatch, Desktop desktop) : base(game, screenManager,  spriteBatch, desktop)
    {
        this.game = game;
        this.screenManager = screenManager;
        this.spriteBatch = spriteBatch;
        this.desktop = desktop;
        this.content = game.Content;
    }

    public override void Initialize()
    {
        
    }

    public override void LoadContent(ContentManager content)
    {
        map = Map.Load(content.RootDirectory +  @"\Tilemaps\player_farm_reduced.tmx", content);
        tileSet = map.Tilesets.Values.ElementAt(0).Texture;
        
    }

    public override void Update(GameTime gameTime)
    {
        KeyboardState keyboardState = Keyboard.GetState();
        MouseState mouseState = Mouse.GetState();

        // Sprawdź, czy wciśnięto klawisz Ctrl
        bool isCtrlPressed = keyboardState.IsKeyDown(Keys.LeftControl) || keyboardState.IsKeyDown(Keys.RightControl);

        if (isCtrlPressed)
        {
            // Oblicz różnicę w ScrollWheelValue (delta)
            int scrollDelta = mouseState.ScrollWheelValue - _previousScrollValue;

            // Zmień zoomLevel w oparciu o różnicę, zastosuj zoomSpeed dla płynności
            _zoomLevel += scrollDelta * zoomSpeed;

            // Ogranicz wartość zoomLevel (min i max)
            _zoomLevel = Math.Clamp(_zoomLevel, MinZoom, MaxZoom);

            // Zapisz bieżący stan scrolla
            _previousScrollValue = mouseState.ScrollWheelValue;
        }
    }

    public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
    {
        graphicsDevice.Clear(Color.Aqua);
        spriteBatch.Begin(samplerState: SamplerState.PointClamp);

        if (map == null) return;
        
        // Thread safety
        lock (map)
        {
            // Obliczenie wymiarów viewportu
            int viewportWidth = (int)(graphicsDevice.Viewport.Width / _zoomLevel);
            int viewportHeight = (int)(graphicsDevice.Viewport.Height / _zoomLevel);
            int scaledTileWidth = (int)(map.TileWidth * _zoomLevel);
            int scaledTileHeight = (int)(map.TileHeight * _zoomLevel);
                
            Rectangle viewport = new Rectangle(
                0,
                0,
                viewportWidth,
                viewportHeight
            );

            Vector2 viewportPosition = Vector2.Zero;
            Vector2 viewportTileStart = viewportPosition / new Vector2(map.TileWidth, map.TileHeight);
            Vector2 viewportTileEnd = viewportTileStart + new Vector2(viewportWidth, viewportHeight);
            foreach (var layer in map.Layers.Values)
            {
                Rectangle tileRect = new Rectangle();
                for (int y = (int)viewportTileStart.Y; y <= (int)viewportTileEnd.Y; y++)
                {
                    for (int x = (int)viewportTileStart.X; x <= (int)viewportTileEnd.X; x++)
                    {
                        if (x < 0 || x >= layer.Width || y < 0 || y >= layer.Height) continue;
                        try
                        {
                            int tileIndex = layer.GetTile(x, y);
                            if (tileIndex >= 0 && map.Tilesets.Any() && map.Tilesets.First().Value.MapTileToRect(tileIndex, ref tileRect))
                            {
                                Vector2 position = new Vector2(
                                    x * scaledTileWidth,
                                    y * scaledTileHeight
                                );

                                spriteBatch.Draw(
                                    tileSet,
                                    position,
                                    tileRect,
                                    Color.White,
                                    rotation: 0f,
                                    origin: Vector2.Zero,
                                    scale: _zoomLevel,
                                    effects: SpriteEffects.None,
                                    layerDepth: 0f
                                );
                            }
                        }
                        catch
                        {
                            // Ignoruj wyjątki w metodzie GetTile
                            continue;
                        }
                    }
                }
            }
        }
        spriteBatch.End();
    }

    public override void Unload()
    {
        
    }
}