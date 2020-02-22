using System.Threading.Tasks;
using System.Numerics;
using System.Drawing;

namespace Tortuga.Components
{
    public class UserInterface : Mesh
    {
        public UserInterface()
        {
            this.ActiveMaterial = Global.Instance.Materials["UserInterface"];
            var task = Task.Run(async () =>
            {
                await this.SetVertices(new Graphics.Vertex[]{
                    new Graphics.Vertex{
                        Position = new Vector3(-1, -1, 0)
                    },
                    new Graphics.Vertex{
                        Position = new Vector3(1, -1, 0)
                    },
                    new Graphics.Vertex{
                        Position = new Vector3(1, 1, 0)
                    },
                    new Graphics.Vertex{
                        Position = new Vector3(-1, 1, 0)
                    }
                });
                await this.SetIndices(new uint[]{
                    0, 1, 2,
                    2, 0, 3
                });
            });
            task.Wait();
        }

        public void UpdateUserInterface(Color[] pixels)
        {
            var setInfo = Global.Instance.MaterialSets["UserInterface_Albedo"];

            var pixelLen = this.ActiveMaterial.SetImages[0].Width * this.ActiveMaterial.SetImages[0].Height;
            if (pixelLen != pixels.Length)
            {
                System.Console.WriteLine("Call 'UpdateUserInterface(pixels, width, height)' before calling this method with wrong image size");
                return;
            }
            this.ActiveMaterial.UpdateSampledImage(setInfo, pixels);
        }

        public void UpdateUserInterface(Color[] pixels, uint width, uint height)
        {
            var setInfo = Global.Instance.MaterialSets["UserInterface_Albedo"];

            if (width * height != pixels.Length)
                throw new System.Exception("Pixel length does not match image width and height");

            this.ActiveMaterial.UpdateSampledImageSize(setInfo, width, height, 1);
            this.ActiveMaterial.UpdateSampledImage(setInfo, pixels);
        }
    }
}