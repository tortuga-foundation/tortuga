using System;
using System.Numerics;
using static Tortuga.Utils.BulletPhysics.BulletPhysicsLoader;

namespace Tortuga.Utils.BulletPhysics
{
    internal class BtDispatcherInfo
    {
        protected IntPtr _handle;

        private delegate IntPtr btDispatcherInfo_new_T();
        private btDispatcherInfo_new_T _btDispatcherInfo_new = LoadFunction<btDispatcherInfo_new_T>("btDispatcherInfo_new");
        public BtDispatcherInfo()
        {
            _handle = _btDispatcherInfo_new();
        }
        private delegate void btDispatcherInfo_destroy_T(IntPtr reference);
        private btDispatcherInfo_destroy_T _btDispatcherInfo_destroy = LoadFunction<btDispatcherInfo_destroy_T>("btDispatcherInfo_destroy");
        ~BtDispatcherInfo()
        {
            _btDispatcherInfo_destroy(_handle);
        }
    }
}