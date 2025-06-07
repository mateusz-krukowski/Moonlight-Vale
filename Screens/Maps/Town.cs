using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Squared.Tiled;

namespace Moonlight_Vale.Screens.Maps;

public class Town : IMap
{
    public Map TileMap { get; }
    public string PathToTileMap { get; }
    public HashSet<int> PasableTileIds { get; set; }
    public (int, int) PlayerSpawnPoint { get; set; }
    public List<(int, int)> Portals { get; set; }
    public OverworldScreen OverworldScreen { get; }
    public void DrawMap(SpriteBatch spriteBatch)
    {
        throw new System.NotImplementedException();
    }

    public void LoadContent(ContentManager content)
    {
        throw new System.NotImplementedException();
    }
}