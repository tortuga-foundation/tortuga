namespace Tortuga.Graphics.Json
{
    /// <summary>
    /// json image type
    /// </summary>
    public class JsonImage : Core.JsonBaseType
    {
        /// <summary>
        /// image path
        /// </summary>
        public string Data { get; set; }
    }

    /// <summary>
    /// json data object for image channel type
    /// </summary>
    public class JsonChannelsData
    {
        /// <summary>
        /// red channel image path
        /// </summary>
        public string R { get; set; }
        /// <summary>
        /// green channel image path
        /// </summary>
        public string G { get; set; }
        /// <summary>
        /// blue channel image path
        /// </summary>
        public string B { get; set; }
    }

    /// <summary>
    /// json image channel type
    /// </summary>
    public class JsonImageChannels : Core.JsonBaseType
    {
        /// <summary>
        /// data for image channel type
        /// </summary>
        public JsonChannelsData Data { get; set; }
    }
}