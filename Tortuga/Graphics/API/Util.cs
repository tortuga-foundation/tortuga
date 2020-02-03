using System.Text;

namespace Tortuga.Graphics.API
{
    internal static class Util
    {
        internal static unsafe string GetString(byte* stringStart)
        {
            int characters = 0;
            while (stringStart[characters] != 0)
                characters++;

            return Encoding.UTF8.GetString(stringStart, characters);
        }
    }
}