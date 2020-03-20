using System.Threading.Tasks;

namespace Tortuga.Core
{
    /// <summary>
    /// base component
    /// NOTE: please use BaseComponent.Create instead of constructor
    /// </summary>
    public class BaseComponent
    {
        /// <summary>
        /// returns the entity, this component is attached to
        /// </summary>
        public Entity MyEntity => _myEntity;
        private Entity _myEntity;

        /// <summary>
        /// this method runs once per frame
        /// </summary>
        public virtual async Task Update() { await Task.Run(() => { }); }
        /// <summary>
        /// This method runs when a component is attached to an entity
        /// </summary>
        public virtual async Task OnEnable() { await Task.Run(() => { }); }
        /// <summary>
        /// This method runs when a component is removed from an entity
        /// </summary>
        public virtual async Task OnDisable() { await Task.Run(() => { }); }

        /// <summary>
        /// This method is used to create a new component and setup all objects required for the component
        /// </summary>
        /// <param name="e">entity this component is attached to</param>
        /// <typeparam name="T">type of component</typeparam>
        /// <returns>an instance of the component</returns>
        public static T Create<T>(Entity e) where T : BaseComponent, new()
        {
            var t = new T();
            t._myEntity = e;
            return t;
        }
    }
}