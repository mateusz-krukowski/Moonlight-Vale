using System.Diagnostics.CodeAnalysis;

namespace Moonlight_Vale
{
    public class Program()
    {
        [SuppressMessage("ReSharper.DPA", "DPA0002: Excessive memory allocations in SOH", MessageId = "type: Enumerator[System.String,Squared.Tiled.Tileset]; size: 17821MB")]
        public static void Main(string[] args)
        {
            var game = new MoonlightVale();
            game.Run();
        }
    }
}