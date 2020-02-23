using System.Threading.Tasks;
using System.Numerics;
using System.Drawing;

namespace Tortuga.Components
{
    public class UserInterface : Mesh
    {
        public UserInterface()
        {
            this.ActiveMaterial = new Graphics.Material(
                new Graphics.Shader(
                    "Assets/Shaders/UserInterface/UserInterface.vert",
                    "Assets/Shaders/UserInterface/UserInterface.frag"
                )
            );
            this.ActiveMaterial.CreateSampledImage("albedo", 1, 1);

            //copy data
            var task = Task.Run(async () =>
            {
                await this.ActiveMaterial.UpdateSampledImage(
                    "albedo",
                    Graphics.Image.SingleColor(System.Drawing.Color.White)
                );
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
                    0, 2, 1,
                    2, 0, 3
                });
            });
            task.Wait();
        }
    }
}