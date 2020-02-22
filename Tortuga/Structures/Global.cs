

using System.Collections.Generic;
using Tortuga.Graphics;
using System.Drawing;

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
            var albedoIndex = Materials["Simple"].CreateSampledImage(1, 1);
            Materials["Simple"].UpdateSampledImage(albedoIndex[0], new Color[] { Color.LightSlateGray });
            Materials.Add(
                "UserInterface",
                new Material(
                    "Assets/Shaders/UserInterface/UserInterface.vert",
                    "Assets/Shaders/UserInterface/UserInterface.frag",
                    false
                )
            );


            _instance = this;
        }
    }
}