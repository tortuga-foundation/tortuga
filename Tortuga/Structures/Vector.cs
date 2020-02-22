namespace Tortuga
{

    [System.Serializable]
    public struct IntVector2D
    {
        public int x, y;

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            return base.ToString();
        }

        public static bool operator ==(IntVector2D left, IntVector2D right)
        {
            return (
                left.x == right.x &&
                left.y == right.y
            );
        }
        public static bool operator !=(IntVector2D left, IntVector2D right)
        {
            return (
                left.x != right.x ||
                left.y != right.y
            );
        }
    }
    [System.Serializable]
    public struct IntVector3D
    {
        public int x, y, z;

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            return base.ToString();
        }

        public static bool operator ==(IntVector3D left, IntVector3D right)
        {
            return (
                left.x == right.x &&
                left.y == right.y &&
                left.z == right.z
            );
        }
        public static bool operator !=(IntVector3D left, IntVector3D right)
        {
            return (
                left.x != right.x ||
                left.y != right.y ||
                left.z != right.z
            );
        }
    }
    [System.Serializable]
    public struct IntVector4D
    {
        public int x, y, z, w;

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            return base.ToString();
        }

        public static bool operator ==(IntVector4D left, IntVector4D right)
        {
            return (
                left.x == right.x &&
                left.y == right.y &&
                left.z == right.z &&
                left.w == right.w
            );
        }
        public static bool operator !=(IntVector4D left, IntVector4D right)
        {
            return (
                left.x != right.x ||
                left.y != right.y ||
                left.z != right.z ||
                left.w != right.w
            );
        }
    }
}