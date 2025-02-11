using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace MoonlightVale.Player;

public class Player
{
    //Fields
    private Texture2D spriteSheet;
    private Vector2 position;
    private Vector2 velocity;
    private SpriteEffects spriteEffect;
    private int frame;
    private float animationTimer;
    private float animationSpeed = 0.2f;
    private float zoom = 4.0f;
    private int currentRow;
    private bool ascending = true;
    
    private static float speed = 200f;
    private static readonly int spriteWidth = 16;
    private static readonly int spriteHeight = 24;
    
    public static int SpriteWidth => spriteWidth;
    public static int SpriteHeight => spriteHeight;

    
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

    public int Frame 
    { 
        get => frame; 
        set => frame = value; 
    }

    public float AnimationTimer 
    { 
        get => animationTimer; 
        set => animationTimer = value; 
    }

    public float AnimationSpeed 
    { 
        get => animationSpeed; 
        set => animationSpeed = value; 
    }

    public float Zoom
    {
        get => zoom;
        set => zoom = value;
    }

    public SpriteEffects SpriteEffect 
    { 
        get => spriteEffect; 
        set => spriteEffect = value; 
    }

    public int CurrentRow 
    { 
        get => currentRow; 
        set => currentRow = value; 
    }

    public bool Ascending 
    { 
        get => ascending; 
        set => ascending = value; 
    }

    public Player(Vector2 startPosition)
    {
        position = startPosition;
        frame = 1;
        animationTimer = 0;
        currentRow = 0;
        spriteEffect = SpriteEffects.None;
    }

    public void LoadContent(ContentManager content, string spriteSheetPath)
    {
        spriteSheet = content.Load<Texture2D>(spriteSheetPath);
    }

    public void Update(GameTime gameTime, KeyboardState keyboard)
    {
        float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
        velocity = Vector2.Zero;

        if (keyboard.IsKeyDown(Keys.W))
        {
            velocity.Y = -Speed;
            currentRow = 2; // Animacja chodzenia w górę
            UpdateAnimation(deltaTime);
        }
        else if (keyboard.IsKeyDown(Keys.S))
        {
            velocity.Y = Speed;
            currentRow = 0; // Animacja chodzenia w dół
            UpdateAnimation(deltaTime);
        }
        else if (keyboard.IsKeyDown(Keys.A))
        {
            velocity.X = -Speed;
            spriteEffect = SpriteEffects.None;
            currentRow = 1; // Animacja chodzenia w lewo
            UpdateAnimation(deltaTime);
        }
        else if (keyboard.IsKeyDown(Keys.D))
        {
            velocity.X = Speed;
            spriteEffect = SpriteEffects.FlipHorizontally;
            currentRow = 1; // Animacja chodzenia w prawo (odbicie lustrzane)
            UpdateAnimation(deltaTime);
        }

        if (velocity == Vector2.Zero)
        {
            frame = 1; // Reset do pozycji neutralnej
        }

        position += velocity * deltaTime;
        
        
    }

    private void UpdateAnimation(float deltaTime)
    {
        animationTimer += deltaTime;
        if (animationTimer >= animationSpeed)
        {
            animationTimer = 0;

            if (ascending)
            {
                frame++;
                if (frame == 2) ascending = false;
            }
            else
            {
                frame--;
                if (frame == 0) ascending = true;
            }
        }
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        Rectangle sourceRect = new Rectangle(frame * SpriteWidth, currentRow * SpriteHeight, SpriteWidth, SpriteHeight);
        spriteBatch.Draw(spriteSheet, position, sourceRect, Color.White, 0, Vector2.Zero, this.Zoom, spriteEffect, 0);
    }
}
