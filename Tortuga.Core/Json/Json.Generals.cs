namespace Tortuga.Core
{
    /// <summary>
    /// base json data structure
    /// </summary>
    public class JsonBaseType
    {
        /// <summary>
        /// Type of json object 
        /// </summary>
        public string Type { get; set; }
    }

    /// <summary>
    /// Json int structure
    /// </summary>
    public class JsonInt32 : JsonBaseType
    {
        /// <summary>
        /// Int value stored in json
        /// </summary>
        public int Data { get; set; }
    }

    /// <summary>
    /// Json float structure
    /// </summary>
    public class JsonFloat : JsonBaseType
    {
        /// <summary>
        /// float value stored in json
        /// </summary>
        public float Data { get; set; }
    }
}