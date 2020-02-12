namespace Tortuga.Math
{
    [System.Serializable]
    public struct Vector2
    {
        public float x, y;
    }

    [System.Serializable]
    public struct Vector3
    {
        public float x, y, z;

        public Vector3(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }
    }

    [System.Serializable]
    public struct Vector4
    {
        public float x, y, z, w;
    }

    [System.Serializable]
    public struct IntVector2D
    {
        public int x, y;
    }
    [System.Serializable]
    public struct IntVector3D
    {
        public int x, y, z;
    }
    [System.Serializable]
    public struct IntVector4D
    {
        public int x, y, z, w;
    }
}