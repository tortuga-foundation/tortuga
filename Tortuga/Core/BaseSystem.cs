using System.Threading.Tasks;

namespace Tortuga.Core
{
    /// <summary>
    /// Base system class
    /// NOTE: please use BaseSystem.Create instead of constructor
    /// </summary>
    public abstract class BaseSystem
    {
        /// <summary>
        /// A referance to the scene this system is attached to
        /// </summary>
        public Scene MyScene => _scene;
        private Scene _scene;

        /// <summary>
        /// Update method, this runs once per frame
        /// </summary>
        public abstract Task Update();

        /// <summary>
        /// this method runs on all systems when a scene is loaded and the system is inside the scene
        /// </summary>
        public abstract void OnEnable();
        /// <summary>
        /// this method runs on all systems when a scene is unloaded or changed and the system is inside the scene
        /// </summary>
        public abstract void OnDisable();

        /// <summary>
        /// This is used to create a new system and setup all objects required for the system
        /// </summary>
        /// <param name="scene">The scene this system belongs to</param>
        /// <typeparam name="T">type of the system</typeparam>
        /// <returns>an instance of the system</returns>
        public static T Create<T>(Scene scene) where T : BaseSystem, new()
        {
            var t = new T();
            t._scene = scene;
            return t;
        }
    }
}