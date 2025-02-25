using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Squared.Tiled;

namespace Moonlight_Vale.Entity;

public class Player
{
    private Texture2D spriteSheet;
    private Vector2 position;
    private Vector2 velocity;
    private SpriteEffects spriteEffect;
    private Map map;
    private int frame;
    private float animationTimer;
    private float animationSpeed = 0.2f;
    private float zoom = 4.0f;
    private int currentRow;
    private bool ascending = true;

    private float speed = 200f;
    private const int spriteWidth = 16;
    private const int spriteHeight = 24;

    private int _selectedItem = 1;
    
    public float UpBorder { get; private set; }
    public float DownBorder { get; private set; }
    public float LeftBorder { get; private set; }
    public float RightBorder { get; private set; }

    // Properties
    public float Speed
    {
        get => speed;
        set => speed = value;
    }

    public Vector2 Position
    {
        get => position;
        set => position = value;
    }

    public Vector2 Velocity
    {
        get => velocity;
        set => velocity = value;
    }
    

    public float Zoom
    {
        get => zoom;
        set => zoom = value;
    }
    
    public int SpriteWidth
    {
        get => spriteWidth;
    }

    public int SpriteHeight
    {
        get => spriteHeight;
    }

    public int SelectedItem
    {
        get => _selectedItem;
        set => _selectedItem = value < 10 && value >=0? value: 0;
    }

    public Player(Vector2 startPosition, Map map)
    {
        position = startPosition;
        frame = 1;
        animationTimer = 0;
        currentRow = 0;
        spriteEffect = SpriteEffects.None;
        this.map = map;
    }

    public void LoadContent(ContentManager content, string spriteSheetPath)
    {
        spriteSheet = content.Load<Texture2D>(spriteSheetPath);
    }

    public void Update(GameTime gameTime, KeyboardState keyboard)
    {
        float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
        velocity = Vector2.Zero;

        switch (keyboard)
        {
            case var k when k.IsKeyDown(Keys.W):
                Move(Vector2.UnitY * -Speed, deltaTime, 2);
                break;
            case var k when k.IsKeyDown(Keys.S):
                Move(Vector2.UnitY * Speed, deltaTime, 0);
                break;
            case var k when k.IsKeyDown(Keys.A):
                Move(Vector2.UnitX * -Speed, deltaTime, 1, SpriteEffects.None);
                break;
            case var k when k.IsKeyDown(Keys.D):
                Move(Vector2.UnitX * Speed, deltaTime, 1, SpriteEffects.FlipHorizontally);
                break;
            
            default:
                frame = 1; // Reset to neutral position
                break;
        }

        position += velocity * deltaTime;

        switch (keyboard) //second switch allows changing item while moving
        {
            case var k when k.IsKeyDown(Keys.D1):
                this.SelectedItem = 0;
                break;
            case var k when k.IsKeyDown(Keys.D2):
                this.SelectedItem = 1;
                break;
            case var k when k.IsKeyDown(Keys.D3):
                this.SelectedItem = 2;
                break;
            case var k when k.IsKeyDown(Keys.D4):
                this.SelectedItem = 3;
                break;
            case var k when k.IsKeyDown(Keys.D5):
                this.SelectedItem = 4;
                break;
            case var k when k.IsKeyDown(Keys.D6):
                this.SelectedItem = 5;
                break;
            case var k when k.IsKeyDown(Keys.D7):
                this.SelectedItem = 6;
                break;
            case var k when k.IsKeyDown(Keys.D8):
                this.SelectedItem = 7;
                break;
            case var k when k.IsKeyDown(Keys.D9):
                this.SelectedItem = 8;
                break;
            case var k when k.IsKeyDown(Keys.D0):
                this.SelectedItem = 9;
                break;
        }
    }

    private void Move(Vector2 direction, float deltaTime, int row, SpriteEffects effect = SpriteEffects.None)
    {
        velocity = direction;
        currentRow = row;
        spriteEffect = effect;

        // Obliczanie pozycji granic
        UpBorder = position.Y;
        DownBorder = position.Y + spriteHeight * zoom;
        LeftBorder = position.X;
        RightBorder = position.X + spriteWidth * zoom;

        // Sprawdzanie, czy bohater może wejść na dany kafelek

        
            position += velocity * deltaTime;
            UpdateAnimation(deltaTime);
        
    }
    
    private bool CanMoveToTile(Vector2 borderPosition)
    {
        Vector2 tileIndex = GetTileIndex(borderPosition);
        int? tileId = map.Layers.Values.First().GetTile((int)tileIndex.X, (int)tileIndex.Y);
        return tileId == 1; // Tylko kafelki o ID 1 są dozwolone
    }
    
    private Vector2 GetTileIndex(Vector2 position)
    {
        int tileX = (int)(position.X / (map.TileWidth * zoom));
        int tileY = (int)(position.Y / (map.TileHeight * zoom));
        return new Vector2(tileX, tileY);
    }

    private void UpdateAnimation(float deltaTime)
    {
        animationTimer += deltaTime;
        if (animationTimer >= animationSpeed)
        {
            animationTimer = 0;
            frame = ascending ? frame + 1 : frame - 1;
            ascending = frame != 2 && (frame == 0 || ascending);
        }
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        var sourceRect = new Rectangle(frame * SpriteWidth, currentRow * SpriteHeight, SpriteWidth, SpriteHeight);
        spriteBatch.Draw(spriteSheet, position, sourceRect, Color.White, 0, Vector2.Zero, zoom, spriteEffect, 0);
    }
}