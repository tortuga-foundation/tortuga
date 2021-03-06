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
        /// singleton instance of engine
        /// </summary>
        public static Engine Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new Engine();

                return _instance;
            }
        }
        private static Engine _instance;

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
        private Engine()
        {
            if (_instance == null)
                _instance = this;
            else
                throw new System.Exception("there can only be one instance of engine");

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
#if TORTUGA_PROFILER
                var profiler = new System.Diagnostics.Stopwatch();
                profiler.Start();
#endif
                float oldTime = 0.0f;
                float currentTime = 0.0f;
                while (this.IsRunning)
                {
                    try
                    {
                        #region Compute Delta Time

                        oldTime = currentTime;
                        currentTime = stopWatch.ElapsedMilliseconds;
                        Time.DeltaTime = (currentTime - oldTime) / 1000.0f;

                        #endregion

                        #region Update Modules

                        foreach (var module in _modules)
                            module.Update();

                        #endregion

                        // used by early update, update & late update
                        var tasks = new List<Task>();

                        #region Early Update

                        tasks = new List<Task>();
#if TORTUGA_PROFILER
                        profiler.Restart();
#endif

                        foreach (var system in _activeScene.Systems.Values)
                        {
#if TORTUGA_PROFILER
                            tasks.Add(Task.Run(async () =>
                            {
                                await system.EarlyUpdate();
                                system.CreateOrUpdateDuration("EarlyUpdate", profiler.ElapsedMilliseconds);
                            }));
#else
                            tasks.Add(system.EarlyUpdate());
#endif
                        }

                        foreach (var entity in _activeScene.Entities)
                        {
                            foreach (var component in entity.Components)
                            {
#if TORTUGA_PROFILER
                                tasks.Add(Task.Run(async () =>
                                {
                                    await component.Value.EarlyUpdate();
                                    component.Value.CreateOrUpdateDuration("EarlyUpdate", profiler.ElapsedMilliseconds);
                                }));
#else
                                tasks.Add(component.Value.EarlyUpdate());
#endif
                            }
                        }
                        Task.WaitAll(tasks.ToArray());

                        #endregion

                        #region OnGui

                        tasks = new List<Task>();
#if TORTUGA_PROFILER
                        profiler.Restart();
#endif

                        foreach (var system in _activeScene.Systems.Values)
                        {
#if TORTUGA_PROFILER
                            tasks.Add(Task.Run(async () =>
                            {
                                await system.OnGui();
                                system.CreateOrUpdateDuration("OnGui", profiler.ElapsedMilliseconds);
                            }));
#else
                            tasks.Add(system.OnGui());
#endif
                        }

                        foreach (var entity in _activeScene.Entities)
                        {
                            foreach (var component in entity.Components)
                            {
#if TORTUGA_PROFILER
                                tasks.Add(Task.Run(async () =>
                                {
                                    await component.Value.OnGui();
                                    component.Value.CreateOrUpdateDuration("OnGui", profiler.ElapsedMilliseconds);
                                }));
#else
                                tasks.Add(component.Value.OnGui());
#endif
                            }
                        }
                        Task.WaitAll(tasks.ToArray());

                        #endregion

                        #region Update

                        tasks = new List<Task>();
#if TORTUGA_PROFILER
                        profiler.Restart();
#endif

                        foreach (var system in _activeScene.Systems.Values)
                        {
#if TORTUGA_PROFILER
                            tasks.Add(Task.Run(async () =>
                            {
                                await system.Update();
                                system.CreateOrUpdateDuration("Update", profiler.ElapsedMilliseconds);
                            }));
#else
                            tasks.Add(system.Update());
#endif
                        }

                        foreach (var entity in _activeScene.Entities)
                        {
                            foreach (var component in entity.Components)
                            {
#if TORTUGA_PROFILER
                                tasks.Add(Task.Run(async () =>
                                {
                                    await component.Value.Update();
                                    component.Value.CreateOrUpdateDuration("Update", profiler.ElapsedMilliseconds);
                                }));
#else
                                tasks.Add(component.Value.Update());
#endif
                            }
                        }
                        Task.WaitAll(tasks.ToArray());

                        #endregion

                        #region Late Update

                        tasks = new List<Task>();
#if TORTUGA_PROFILER
                        profiler.Restart();
#endif

                        foreach (var system in _activeScene.Systems.Values)
                        {
#if TORTUGA_PROFILER
                            tasks.Add(Task.Run(async () =>
                            {
                                await system.LateUpdate();
                                system.CreateOrUpdateDuration("LateUpdate", profiler.ElapsedMilliseconds);
                            }));
#else
                            tasks.Add(system.LateUpdate());
#endif
                        }

                        foreach (var entity in _activeScene.Entities)
                        {
                            foreach (var component in entity.Components)
                            {
#if TORTUGA_PROFILER
                                tasks.Add(Task.Run(async () =>
                                {
                                    await component.Value.LateUpdate();
                                    component.Value.CreateOrUpdateDuration("LateUpdate", profiler.ElapsedMilliseconds);
                                }));
#else
                                tasks.Add(component.Value.LateUpdate());
#endif
                            }
                        }
                        Task.WaitAll(tasks.ToArray());

                        #endregion

                        #region Clean Up

                        var removalTasks = new List<Task>();
                        foreach (var entity in _activeScene.Entities)
                            removalTasks.Add(entity.RemoveAllMarkedForRemoval());
                        Task.WaitAll(removalTasks.ToArray());

                        #endregion

                        #region Frame Limiter

                        if (Settings.MaxLoopsPerSecond > 0)
                        {
                            int waitTime = System.Convert.ToInt32(
                                System.MathF.Round(
                                (
                                    1000.0f / Settings.MaxLoopsPerSecond) -
                                    (stopWatch.ElapsedMilliseconds - currentTime)
                                )
                            );
                            if (waitTime > 0)
                                System.Threading.Thread.Sleep(waitTime);
                        }

                        #endregion
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

        /// <summary>
        /// Get's a module from the engine
        /// </summary>
        /// <typeparam name="T">type of module to get</typeparam>
        public T GetModule<T>() where T : Core.BaseModule, new()
        {
            var index = _modules.FindIndex(m => m.GetType() == typeof(T));
            if (index == -1)
                return null;
            return _modules[index] as T;
        }
    }
}