#pragma warning disable 1711

#if TORTUGA_PROFILER
using System.Collections.Generic;
#endif
using System.Threading.Tasks;

namespace Tortuga.Core
{
    /// <summary>
    /// Base system class
    /// NOTE: please use BaseSystem.Create instead of constructor
    /// </summary>
    public class BaseSystem
    {
#if TORTUGA_PROFILER

        /// <summary>
        /// Records how long each function takes for debugging
        /// </summary>
        /// <typeparam name="string">function name</typeparam>
        /// <typeparam name="long">time in miliseconds</typeparam>
        public Dictionary<string, long> FunctionDuration = new Dictionary<string, long>();

        internal void CreateOrUpdateDuration(string key, long duration)
        {
            if (FunctionDuration.ContainsKey(key) == false)
                FunctionDuration.Add(key, duration);
            else
                FunctionDuration[key] = duration;
        }
        
#endif

        /// <summary>
        /// A referance to the scene this system is attached to
        /// </summary>
        public Scene MyScene => _scene;
        private Scene _scene;

        /// <summary>
        /// Update method, this runs once per frame
        /// </summary>
        public virtual Task Update() => Task.Run(() => {});

        /// <summary>
        /// Runs every frame before update method
        /// </summary>
        public virtual Task EarlyUpdate() => Task.Run(() => {});

        /// <summary>
        /// All GUI commands must run inside this function
        /// </summary>
        public virtual Task OnGui() => Task.Run(() => {});

        /// <summary>
        /// Runs every frame after update method
        /// </summary>
        public virtual Task LateUpdate() => Task.Run(() => {});

        /// <summary>
        /// this method runs on all systems when a scene is loaded and the system is inside the scene
        /// </summary>
        public virtual void OnEnable(){}
        /// <summary>
        /// this method runs on all systems when a scene is unloaded or changed and the system is inside the scene
        /// </summary>
        public virtual void OnDisable(){}

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