using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Moonlight_Vale.Entity
{
    public class Npc
    {
        private const int SPRITE_WIDTH = 16;
        private const int SPRITE_HEIGHT = 24;
        private Texture2D spriteSheet;
        private Texture2D sprite;
        private ContentManager _contentManager;
        
        public Vector2 Position { get; set; }
        public float Zoom { get; set; } = 2.0f;
    }
}