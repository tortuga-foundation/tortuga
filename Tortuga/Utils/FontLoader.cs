using System.Threading.Tasks;
using System.Text.Json;

namespace Tortuga.Utils
{
    public static class FontLoader
    {
        public static Task<Graphics.GUI.Font> Load()
        {
            var font = new Graphics.GUI.Font();

            return Task.FromResult(font);
        }
    }
}