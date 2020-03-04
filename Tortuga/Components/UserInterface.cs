using System.Threading.Tasks;
using System.Numerics;

namespace Tortuga.Components
{
    public class UserInterface : Mesh
    {
        private static Graphics.Material _cachedMaterial = Graphics.Material.Load("Assets/Material/UI.json");
        public override Graphics.Material ActiveMaterial
        {
            get
            {
                if (_cachedMaterial == null)
                    _cachedMaterial = Graphics.Material.Load("Assets/Material/UI.json");
                return _cachedMaterial;
            }
        }

        public UserInterface()
        {
            this.SetVertices(
                new Graphics.Vertex[]{
                    new Graphics.Vertex{
                        Position = new Vector3(-1, -1, 0),
                        TextureCoordinates = new Vector2(0, 0),
                        Normal = new Vector3(0, 0, 1)
                    },
                    new Graphics.Vertex{
                        Position = new Vector3(1, -1, 0),
                        TextureCoordinates = new Vector2(1, 0),
                        Normal = new Vector3(0, 0, 1)
                    },
                    new Graphics.Vertex{
                        Position = new Vector3(1, 1, 0),
                        TextureCoordinates = new Vector2(1, 1),
                        Normal = new Vector3(0, 0, 1)
                    },
                    new Graphics.Vertex{
                        Position = new Vector3(-1, 1, 0),
                        TextureCoordinates = new Vector2(0, 1),
                        Normal = new Vector3(0, 0, 1)
                    }
                }
            ).Wait();
            this.SetIndices(
                new uint[]{
                    0, 2, 1,
                    2, 0, 3
                }
            ).Wait();
        }
    
        public Task UpdateImage(Graphics.Image image)
        {
            return this.ActiveMaterial.UpdateSampledImage("Albedo", 0, image);
        }
    }
}