using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Squared.Tiled;
using System.Linq;
using Myra.Graphics2D.UI;

namespace Moonlight_Vale.Screens;

public class OverworldScreen : GameScreen
{
    private Map map;
    private Texture2D tileSet;
    private float zoom = 4.0f;
    private int prevScroll;
    private const float MinZoom = 0.5f, MaxZoom = 10.0f, ZoomSpeed = 0.001f;

    public OverworldScreen(Game game, ScreenManager manager, SpriteBatch batch, Desktop desktop) 
        : base(game, manager, batch, desktop) { }

    public override void Initialize() { }
    
    public override void LoadContent(ContentManager content)
    {
        map = Map.Load(content.RootDirectory + @"\Tilemaps\player_farm_reduced.tmx", content);
        tileSet = map.Tilesets.Values.First().Texture;
    }

    public override void Update(GameTime gameTime)
    {
        var keyboard = Keyboard.GetState();
        var mouse = Mouse.GetState();

        if (keyboard.IsKeyDown(Keys.LeftControl))
        {
            zoom = Math.Clamp(zoom + (mouse.ScrollWheelValue - prevScroll) * ZoomSpeed, MinZoom, MaxZoom);
            prevScroll = mouse.ScrollWheelValue;
        }
    }

    public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
    {
        graphicsDevice.Clear(Color.Aqua);
        if (map == null) return;

        spriteBatch.Begin(samplerState: SamplerState.PointClamp); //removes antialiasing

        foreach (var layer in map.Layers.Values)
        {
            DrawLayer(spriteBatch, layer);
        }

        spriteBatch.End();
    }

    private void DrawLayer(SpriteBatch spriteBatch, Layer layer)
    {
        int scaledTileSize = (int)(map.TileWidth * zoom);
        
        for (int y = 0; y < layer.Height; y++)
        for (int x = 0; x < layer.Width; x++)
        {
            int tileIndex = layer.GetTile(x, y);

            Rectangle tileRect = new();
            if (!map.Tilesets.First().Value.MapTileToRect(tileIndex, ref tileRect)) continue;

            spriteBatch.Draw(tileSet, new Vector2(x, y) * scaledTileSize, tileRect, Color.White, 0, Vector2.Zero, zoom, SpriteEffects.None, 0);
        }
    }

    public override void Unload() { }
}
