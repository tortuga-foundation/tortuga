using System;
using System.Numerics;
using static Tortuga.Utils.BulletPhysics.BulletPhysicsLoader;

namespace Tortuga.Utils.BulletPhysics
{
    internal struct BtBroadphaseProxy
    {
        public readonly IntPtr NativePointer;
        public BtBroadphaseProxy(IntPtr pointer)
        {
            NativePointer = pointer;
        }

        public static implicit operator IntPtr(BtBroadphaseProxy device) => device.NativePointer;
        public static implicit operator BtBroadphaseProxy(IntPtr pointer) => new BtBroadphaseProxy(pointer);
    }

    internal abstract class BtBroadphaseAabbCallback
    {
        protected IntPtr _handle;

        private delegate void btBroadphaseAabbCallback_destroy_T(IntPtr reference); 
        private static btBroadphaseAabbCallback_destroy_T _btBroadphaseAabbCallback_destroy = LoadFunction<btBroadphaseAabbCallback_destroy_T>("btBroadphaseAabbCallback_destroy");

        ~BtBroadphaseAabbCallback()
        {
            _btBroadphaseAabbCallback_destroy(_handle);
        }

        private delegate void btBroadphaseAabbCallback_process_T(IntPtr reference, IntPtr proxy);
        private static btBroadphaseAabbCallback_process_T _btBroadphaseAabbCallback_process = LoadFunction<btBroadphaseAabbCallback_process_T>("btBroadphaseAabbCallback_process");
        public void Process(BtBroadphaseProxy proxy)
        {
            _btBroadphaseAabbCallback_process(_handle, proxy);
        }
    }

    internal abstract class BtBroadphaseRayCallback
    {
        protected IntPtr _handle;

        private delegate void btBroadphaseRayCallback_destroy_T(IntPtr reference); 
        private static btBroadphaseRayCallback_destroy_T _btBroadphaseRayCallback_destroy = LoadFunction<btBroadphaseRayCallback_destroy_T>("btBroadphaseRayCallback_destroy");
        private BtBroadphaseRayCallback(){}
        ~BtBroadphaseRayCallback()
        {
            _btBroadphaseRayCallback_destroy(_handle);
        }

        private delegate void btBroadphaseRayCallback_setRayDirectionInverse_T(IntPtr reference, float x, float y, float z); 
        private static btBroadphaseRayCallback_setRayDirectionInverse_T _btBroadphaseRayCallback_setRayDirectionInverse = LoadFunction<btBroadphaseRayCallback_setRayDirectionInverse_T>("btBroadphaseRayCallback_setRayDirectionInverse");
        private delegate void btBroadphaseRayCallback_getRayDirectionInverse_T(IntPtr reference, out float x, out float y, out float z); 
        private static btBroadphaseRayCallback_getRayDirectionInverse_T _btBroadphaseRayCallback_getRayDirectionInverse = LoadFunction<btBroadphaseRayCallback_getRayDirectionInverse_T>("btBroadphaseRayCallback_getRayDirectionInverse");
        public Vector3 Direction
        {
            set => _btBroadphaseRayCallback_setRayDirectionInverse(_handle, value.X, value.Y, value.Z);
            get
            {
                var rtn = Vector3.Zero;
                _btBroadphaseRayCallback_getRayDirectionInverse(_handle, out rtn.X, out rtn.Y, out rtn.Z);
                return rtn;
            }
        }
    
        private delegate void btBroadphaseRayCallback_setSigns_T(IntPtr reference, uint x, uint y, uint z); 
        private static btBroadphaseRayCallback_setSigns_T _btBroadphaseRayCallback_setSigns = LoadFunction<btBroadphaseRayCallback_setSigns_T>("btBroadphaseRayCallback_setSigns");
        private delegate void btBroadphaseRayCallback_getSigns_T(IntPtr reference, out uint x, out uint y, out uint z); 
        private static btBroadphaseRayCallback_getSigns_T _btBroadphaseRayCallback_getSigns = LoadFunction<btBroadphaseRayCallback_getSigns_T>("btBroadphaseRayCallback_getSigns");
        public uint[] Signs
        {
            set
            {
                if (value.Length != 3)
                    throw new InvalidOperationException("There must be 3 signs!");
                
                _btBroadphaseRayCallback_setSigns(_handle, value[0], value[1], value[2]);
            }
            get
            {
                var rtn = new uint[3];
                _btBroadphaseRayCallback_getSigns(_handle, out rtn[0], out rtn[1], out rtn[2]);
                return rtn;
            }
        }
    
        private delegate void btBroadphaseRayCallback_setLambdaMax_T(IntPtr reference, float value); 
        private static btBroadphaseRayCallback_setLambdaMax_T _btBroadphaseRayCallback_setLambdaMax = LoadFunction<btBroadphaseRayCallback_setLambdaMax_T>("btBroadphaseRayCallback_setLambdaMax");
        private delegate float btBroadphaseRayCallback_getLambdaMax_T(IntPtr reference); 
        private static btBroadphaseRayCallback_getLambdaMax_T _btBroadphaseRayCallback_getLambdaMax = LoadFunction<btBroadphaseRayCallback_getLambdaMax_T>("btBroadphaseRayCallback_getLambdaMax");
        public float LambdaMax
        {
            set => _btBroadphaseRayCallback_setLambdaMax(_handle, value);
            get => _btBroadphaseRayCallback_getLambdaMax(_handle);
        }
    }

    internal abstract class BtBroadphaseInterface : BtBroadphaseAabbCallback
    {
        private delegate void btBroadphaseInterface_destroy_T(IntPtr reference);
        private btBroadphaseInterface_destroy_T _btBroadphaseInterface_destroy = LoadFunction<btBroadphaseInterface_destroy_T>("btBroadphaseInterface_destroy");
        ~BtBroadphaseInterface()
        {
            _btBroadphaseInterface_destroy(_handle);
        }

        private delegate BtBroadphaseProxy btBroadphaseInterface_createProxy_T(
            IntPtr reference,
            float aabbMinX, float aabbMinY, float aabbMinZ,
            float aabbMaxX, float aabbMaxY, float aabbMaxZ,
            int shapeType,
            IntPtr userPtr,
            int collisionFilterGroup,
            int collisionFilterMask,
            IntPtr dispatcher    
        );
        private btBroadphaseInterface_createProxy_T _btBroadphaseInterface_createProxy = LoadFunction<btBroadphaseInterface_createProxy_T>("btBroadphaseInterface_createProxy");
        public BtBroadphaseProxy GetProxy(
            Vector3 aabbMin,
            Vector3 aabbMax,
            int shapeType,
            IntPtr userPtr,
            int collisionFilterGroup,
            int collisionFilterMask,
            IntPtr dispatcher //todo: replace with dispatcher type object
        )
        {
            return _btBroadphaseInterface_createProxy(
                _handle,
                aabbMin.X, aabbMin.Y, aabbMin.Z,
                aabbMax.X, aabbMax.Y, aabbMax.Z,
                shapeType,
                userPtr,
                collisionFilterGroup,
                collisionFilterMask,
                dispatcher
            );
        }

        private delegate void btBroadphaseInterface_destroyProxy_T(IntPtr reference, BtBroadphaseProxy proxy, IntPtr dispatcher);
        private btBroadphaseInterface_destroyProxy_T _btBroadphaseInterface_destroyProxy = LoadFunction<btBroadphaseInterface_destroyProxy_T>("btBroadphaseInterface_destroyProxy");
        public void DestroyProxy(BtBroadphaseProxy proxy, IntPtr dispatcher)
        {
            _btBroadphaseInterface_destroyProxy(_handle, proxy, dispatcher);
        }
    
        

    }
}