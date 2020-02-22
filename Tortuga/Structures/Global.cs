

using System.Collections.Generic;
using Tortuga.Graphics;
using System.Drawing;

namespace Tortuga
{
    public class Global
    {
        public static Global Instance => _instance;
        public Dictionary<string, Material> Materials;
        public Dictionary<string, DescriptorSetObject> MaterialSets;

        private static Global _instance;

        public Global()
        {
            if (_instance != null)
                return;

            Materials = new Dictionary<string, Material>();
            MaterialSets = new Dictionary<string, DescriptorSetObject>();
            //simple material
            {
                Materials.Add(
                    "Simple",
                    new Material(
                        "Assets/Shaders/Simple/Simple.vert",
                        "Assets/Shaders/Simple/Simple.frag"
                    )
                );
                var albedoIndex = Materials["Simple"].CreateSampledImage(1, 1);
                Materials["Simple"].UpdateSampledImage(albedoIndex[0], new Color[] { Color.LightSlateGray });
                MaterialSets.Add("Simple_Albedo", albedoIndex[0]);
            }

            //user interface material
            {
                Materials.Add(
                    "UserInterface",
                    new Material(
                        "Assets/Shaders/UserInterface/UserInterface.vert",
                        "Assets/Shaders/UserInterface/UserInterface.frag",
                        false
                    )
                );
                var albedoIndex = Materials["UserInterface"].CreateSampledImage(1, 1);
                Materials["UserInterface"].UpdateSampledImage(albedoIndex[0], new Color[] { Color.LightSlateGray });
                MaterialSets.Add("UserInterface_Albedo", albedoIndex[0]);
            }


            _instance = this;
        }
    }
}