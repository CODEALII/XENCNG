namespace XENCNG.Engine
{
    public static class FileWalker
    {
        public static List<string> GetAllFiles(string root)
        {
            return Directory.GetFiles(root, "*", SearchOption.AllDirectories).ToList();
        }
    }
}
