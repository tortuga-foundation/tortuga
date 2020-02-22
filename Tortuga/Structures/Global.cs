

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
                var t = Materials["Simple"].UpdateSampledImage(albedoIndex[0], new Color[] { Color.LightSlateGray });
                MaterialSets.Add("Simple_Albedo", albedoIndex[0]);
                t.Wait();
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
                var t = Materials["UserInterface"].UpdateSampledImage(albedoIndex[0], new Color[] { Color.LightSlateGray });
                MaterialSets.Add("UserInterface_Albedo", albedoIndex[0]);
                t.Wait();
            }

            //pbr
            {
                Materials.Add(
                    "PBR",
                    new Material(
                        "Assets/Shaders/PBR/PBR.vert",
                        "Assets/Shaders/PBR/PBR.frag"
                    )
                );
                var materialInfo = Materials["PBR"].CreateUniformData<Graphics.PBR>();
                var t = Materials["PBR"].UpdateUniformData(materialInfo[0], new Graphics.PBR[]{
                    new PBR{
                        Metallic = 0,
                        Rougness = 0.5f
                    }
                });
                MaterialSets.Add("PBR_MATERIAL_INFO", materialInfo[0]);
                t.Wait();
            }


            _instance = this;
        }
    }
}