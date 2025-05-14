using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Squared.Tiled;

namespace Moonlight_Vale.Entity;

public class Player
{
/* TODO
    mechanika stawiania klockow
        player w prawo = x wiekszy
         playerr w lewo = x mniejszy
        player do gory = y mniejszy
        player w dol = y wiekszyw
 */    
    
    private Texture2D spriteSheet;
    private Vector2 position;
    
    private Vector2 velocity;
    private SpriteEffects spriteEffect;
    private Map map;
    private int frame;
    private float animationTimer;
    private float animationSpeed = 0.18f;
    private float zoom = 2.0f;
    private int currentRow;
    private bool ascending = true;

    private float speed = 3200f;
    private const int spriteWidth = 16;
    private const int spriteHeight = 24;
    private const float HeadOffset = 6f; // Nowa stała dla offsetu głowy

    private int _selectedItem = 1;
    
    private Vector2 direction;
    public Vector2 Direction => direction;
    
    public float UpBorder { get; private set; }
    public float DownBorder { get; private set; }
    public float LeftBorder { get; private set; }
    public float RightBorder { get; private set; }

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
        UpdateBorders(); // Inicjalizacja granic
    }

    public void LoadContent(ContentManager content, string spriteSheetPath)
    {
        spriteSheet = content.Load<Texture2D>(spriteSheetPath);
    }

    public void Update(GameTime gameTime, KeyboardState keyboard)
    {
        float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
        velocity = Vector2.Zero;

        
        //movement animation
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
                frame = 1;
                break;
        }

        switch (keyboard)
        {
            case var k when k.IsKeyDown(Keys.D1): this.SelectedItem = 0; break;
            case var k when k.IsKeyDown(Keys.D2): this.SelectedItem = 1; break;
            case var k when k.IsKeyDown(Keys.D3): this.SelectedItem = 2; break;
            case var k when k.IsKeyDown(Keys.D4): this.SelectedItem = 3; break;
            case var k when k.IsKeyDown(Keys.D5): this.SelectedItem = 4; break;
            case var k when k.IsKeyDown(Keys.D6): this.SelectedItem = 5; break;
            case var k when k.IsKeyDown(Keys.D7): this.SelectedItem = 6; break;
            case var k when k.IsKeyDown(Keys.D8): this.SelectedItem = 7; break;
            case var k when k.IsKeyDown(Keys.D9): this.SelectedItem = 8; break;
            case var k when k.IsKeyDown(Keys.D0): this.SelectedItem = 9; break;
        }
        
        switch(keyboard)
        {
            case var k when k.IsKeyDown(Keys.E): HandleTileReplacement() ; break;
        }
    }

    private void HandleTileReplacement()
    {
        
        string directionName = GetDirectionName(Direction);
        
        Vector2 tileIndex = GetTargetTileIndex(position, Direction);

        if (tileIndex.X < 0 || tileIndex.Y < 0)
        {
            Console.WriteLine("Invalid tile index");
            return;
        }

        // Pobierz warstwę mapy
        var layer = map.Layers.Values.FirstOrDefault();
        if (layer == null) return;

        // Zmień ID kafelka na nowe (np. z 0,0 na 0,1)
        int currentTileId = layer.GetTile((int)tileIndex.X, (int)tileIndex.Y);
        if (currentTileId > 0)
        {
            layer.Tiles[(int)(tileIndex.Y * layer.Width + tileIndex.X)] = 12; // Nowy ID kafelka
            Console.WriteLine($"Tile at {tileIndex} changed!");
        }
        
        Console.WriteLine($"Player direction: {directionName}");
        
        
        // pomocnicza funkcja do namierzania kierunku bohatera
        string GetDirectionName(Vector2 direction)
        {
            if (direction == Vector2.Zero)
                return "Idle"; // Gracz się nie porusza

            if (Math.Abs(direction.X) > Math.Abs(direction.Y))
            {
                return direction.X > 0 ? "Right" : "Left";
            }
            else
            {
                return direction.Y > 0 ? "Down" : "Up";
            }
        }
        
    }
    
    private Vector2 GetTargetTileIndex(Vector2 position, Vector2 direction)
    {
        // Oblicz przesunięcie w zależności od kierunku
        Vector2 offset = Vector2.Zero;
        if (direction.X > 0) offset = Vector2.UnitX;         // Right
        else if (direction.X < 0) offset = -Vector2.UnitX;   // Left
        else if (direction.Y > 0) offset = Vector2.UnitY;    // Down
        else if (direction.Y < 0) offset = -Vector2.UnitY;   // Up

        // Przekształć pozycję gracza na indeks kafelka
        Vector2 currentTileIndex = GetTileIndex(position);

        // Przesunięcie w dół o 1 kafelek (z torsu bohatera)
        currentTileIndex.Y += 1;

        // Zastosuj przesunięcie wynikające z kierunku
        return currentTileIndex + offset;
    }
    
    

    private void Move(Vector2 direction, float deltaTime, int row, SpriteEffects effect = SpriteEffects.None)
    {
        spriteEffect = effect;
        currentRow = row;

        // Aktualizacja kierunku gracza
        this.direction = direction != Vector2.Zero ? Vector2.Normalize(direction) : this.direction;

        Vector2 newPosition = position + direction * deltaTime;
        float scaledHeadOffset = HeadOffset * zoom;

        float newUpBorder = newPosition.Y + scaledHeadOffset;
        float newDownBorder = newPosition.Y + (spriteHeight * zoom);
        float newLeftBorder = newPosition.X;
        float newRightBorder = newPosition.X + (spriteWidth * zoom);

        bool canMoveHorizontally = CanMoveToTile(new Vector2(newLeftBorder, newUpBorder)) && 
                                   CanMoveToTile(new Vector2(newRightBorder, newUpBorder));
        bool canMoveVertically = CanMoveToTile(new Vector2(position.X, newUpBorder)) && 
                                 CanMoveToTile(new Vector2(position.X, newDownBorder));

        if (direction.X != 0 && canMoveHorizontally)
        {
            position.X += direction.X * deltaTime;
        }

        if (direction.Y != 0 && canMoveVertically)
        {
            position.Y += direction.Y * deltaTime;
        }

        UpdateAnimation(deltaTime);
        UpdateBorders();
    }


    private void UpdateBorders()
    {
        float scaledHeadOffset = HeadOffset * zoom;
        UpBorder = position.Y + scaledHeadOffset;
        DownBorder = position.Y + spriteHeight * zoom;
        LeftBorder = position.X;
        RightBorder = position.X + spriteWidth * zoom;
    }

    private bool CanMoveToTile(Vector2 borderPosition)
    {
        Vector2 tileIndex = GetTileIndex(borderPosition);
        int? tileId = map.Layers.Values[1].GetTile((int)tileIndex.X, (int)tileIndex.Y);
        return tileId <= 12;
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