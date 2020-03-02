using System.Threading.Tasks;
using System.Numerics;

namespace Tortuga.Components
{
    public class UserInterface : Mesh
    {
        public bool Is3D
        {
            get => _is3D;
            set
            {
                _is3D = value;
                int val = 0;
                if (value)
                    val = 1;
                var t = this.ActiveMaterial.UpdateUniformData<int>("Is3D", val);
                t.Wait();
            }
        }

        private bool _is3D = true;

        public UserInterface()
        {
            this.ActiveMaterial = new Graphics.Material(
                new Graphics.Shader(
                    "Assets/Shaders/UserInterface/UserInterface.vert",
                    "Assets/Shaders/UserInterface/UserInterface.frag"
                ),
                false
            );
            this.ActiveMaterial.CreateUniformData<int>("Is3D");
            this.ActiveMaterial.CreateSampledImage("albedo", 1, 1);

            //copy data
            var task = Task.Run(async () =>
            {
                await this.ActiveMaterial.UpdateUniformData<int>("Is3D", 1);
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