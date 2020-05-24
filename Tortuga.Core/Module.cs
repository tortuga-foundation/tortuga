namespace Tortuga.Core
{
    /// <summary>
    /// Can be used to attach different modules to this game engine
    /// </summary>
    public abstract class BaseModule
    {
        /// <summary>
        /// On module initialize
        /// </summary>
        public abstract void Init();
        /// <summary>
        /// Update module
        /// </summary>
        public abstract void Update();
        /// <summary>
        /// destroy module
        /// </summary>
        public abstract void Destroy();
    }
}