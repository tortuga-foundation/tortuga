using System.Collections.Generic;
using System.Threading.Tasks;

namespace Tortuga
{
    /// <summary>
    /// Engine Singleton class, this is required to run the engine
    /// </summary>
    public class Engine
    {
        /// <summary>
        /// Controlls if the application is running
        /// </summary>
        public bool IsRunning;

        /// <summary>
        /// Returns currently active scene
        /// </summary>
        public Core.Scene CurrentScene => _activeScene;
        private Core.Scene _activeScene;

        private List<Core.BaseModule> _modules;

        /// <summary>
        /// constructor for engine
        /// </summary>
        public Engine()
        {
            _modules = new List<Core.BaseModule>();
        }

        /// <summary>
        /// Main engine loop
        /// </summary>
        /// <returns>Returns task, if not using async await then please use task.Wait()</returns>
        public Task Run()
        {
            IsRunning = true;
            return Task.Run(() =>
            {
                var stopWatch = new System.Diagnostics.Stopwatch();
                stopWatch.Start();
                float oldTime = 0.0f;
                float currentTime = 0.0f;
                while (this.IsRunning)
                {
                    try
                    {
                        oldTime = currentTime;
                        currentTime = stopWatch.ElapsedMilliseconds;
                        Time.DeltaTime = (currentTime - oldTime) / 1000.0f;
                        //update modules
                        foreach (var module in _modules)
                            module.Update();

                        //early update
                        if (_activeScene != null)
                        {
                            var tasks = new List<Task>();
                            foreach (var system in _activeScene.Systems.Values)
                                tasks.Add(system.EarlyUpdate());

                            foreach (var entity in _activeScene.Entities)
                            {
                                foreach (var component in entity.Components)
                                    tasks.Add(component.Value.EarlyUpdate());
                            }
                            Task.WaitAll(tasks.ToArray());
                        }
                        //update
                        if (_activeScene != null)
                        {
                            var tasks = new List<Task>();
                            foreach (var system in _activeScene.Systems.Values)
                                tasks.Add(system.Update());

                            foreach (var entity in _activeScene.Entities)
                            {
                                foreach (var component in entity.Components)
                                    tasks.Add(component.Value.Update());
                            }
                            Task.WaitAll(tasks.ToArray());
                        }
                        //late update
                        if (_activeScene != null)
                        {
                            var tasks = new List<Task>();
                            foreach (var system in _activeScene.Systems.Values)
                                tasks.Add(system.LateUpdate());

                            foreach (var entity in _activeScene.Entities)
                            {
                                foreach (var component in entity.Components)
                                    tasks.Add(component.Value.LateUpdate());
                            }
                            Task.WaitAll(tasks.ToArray());
                        }
                        //clean up marked for removal
                        var removalTasks = new List<Task>();
                        foreach (var entity in _activeScene.Entities)
                            removalTasks.Add(entity.RemoveAllMarkedForRemoval());
                        Task.WaitAll(removalTasks.ToArray());

                        //limiter
                        if (Settings.Core.MaxLoopsPerSecond > 0)
                        {
                            int waitTime = System.Convert.ToInt32(
                                System.MathF.Round(
                                (
                                    1000.0f / Settings.Core.MaxLoopsPerSecond) -
                                    (stopWatch.ElapsedMilliseconds - currentTime)
                                )
                            );
                            if (waitTime > 0)
                                System.Threading.Thread.Sleep(waitTime);
                        }
                    }
                    catch (System.Exception e)
                    {
                        System.Console.WriteLine(e.ToString());
                    }
                }
            });
        }

        /// <summary>
        /// Load a specific scene
        /// </summary>
        /// <param name="scene">Scene to load</param>
        public void LoadScene(Core.Scene scene)
        {
            if (_activeScene != null && _activeScene != new Core.Scene())
                UnloadScene();

            _activeScene = scene;
            foreach (var system in this._activeScene.Systems)
                system.Value.OnEnable();
        }

        /// <summary>
        /// Unload a scene / clear all entities
        /// </summary>
        public void UnloadScene()
        {
            foreach (var system in this._activeScene.Systems)
                system.Value.OnDisable();

            _activeScene = new Core.Scene();
        }

        /// <summary>
        /// Adds a module to the engine
        /// </summary>
        /// <typeparam name="T">module to add</typeparam>
        public void AddModule<T>() where T : Core.BaseModule, new()
        {
            var index = _modules.FindIndex(m => m.GetType() == typeof(T));
            if (index == -1)
            {
                var module = new T();
                module.Init();
                _modules.Add(module);
            }
        }

        /// <summary>
        /// Removes a module from the engine
        /// </summary>
        /// <typeparam name="T">module to remove</typeparam>
        public void RemoveModule<T>() where T : Core.BaseModule, new()
        {
            var index = _modules.FindIndex(m => m.GetType() == typeof(T));
            if (index > -1)
            {
                _modules[index].Destroy();
                _modules.RemoveAt(index);
            }
        }
    }
}