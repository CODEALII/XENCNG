namespace XENCNG.Engine
{
    /// <summary>
    /// Utility class for recursively discovering files in directory structures.
    /// Used for batch encryption/decryption of entire folders.
    /// </summary>
    public static class FileWalker
    {
        /// <summary>
        /// Recursively finds all files in a directory and its subdirectories.
        /// Returns a flat list of all file paths found.
        /// </summary>
        /// <param name="root">Root directory path to search</param>
        /// <returns>List of all file paths found recursively</returns>
        public static List<string> GetAllFiles(string root)
        {
            return Directory.GetFiles(root, "*", SearchOption.AllDirectories).ToList();
        }
    }
}
