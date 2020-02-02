using System;

namespace Tortuga
{
    class Program
    {
        unsafe static void Main(string[] args)
        {
            try
            {
                new Engine().Run();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }
}