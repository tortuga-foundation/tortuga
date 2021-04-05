namespace Tortuga
{
    /// <summary>
    /// Useful methods that extend the string class
    /// </summary>
    public static class StringExtension
    {
        /// <summary>
        /// Convert's a string to char pointer
        /// </summary>
        /// <returns>char pointer</returns>
        public static unsafe char* ToCharPointer(this string str)
        {
            if (string.IsNullOrEmpty(str))
                return null;

            var charArray = str.ToCharArray();

            fixed (char* ptr = charArray)
                return ptr;
        }
    }
}