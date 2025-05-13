using Microsoft.Xna.Framework.Input;
using Myra.Graphics2D.UI;

namespace Moonlight_Vale.UI;

public class HudManager
{
    public KeyboardState Keyboard { get; set; }
    public MouseState Mouse { get; set; }
    public Desktop Desktop { get; set; }
    
    public HudManager(KeyboardState keyboard, MouseState mouse, Desktop desktop)
    {
        Keyboard = keyboard;
        Mouse = mouse;
        Desktop = desktop;
    }
}