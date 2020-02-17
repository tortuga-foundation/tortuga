

using System.Collections.Generic;
using Tortuga.Graphics;

namespace Tortuga
{
    public class Global
    {
        public static Global Instance => _instance;
        public Dictionary<string, Material> Materials;

        private static Global _instance;

        public Global()
        {
            if (_instance != null)
                return;

            Materials = new Dictionary<string, Material>();
            Materials.Add(
                "Simple",
                new Material(
                    "Assets/Shaders/Simple/Simple.vert",
                    "Assets/Shaders/Simple/Simple.frag"
                )
            );
            Materials.Add(
                "UserInterface",
                new Material(
                    "Assets/Shaders/UserInterface/UserInterface.vert",
                    "Assets/Shaders/UserInterface/UserInterface.frag"
                )
            );


            _instance = this;
        }
    }
}