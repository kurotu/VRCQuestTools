using System.IO;

namespace KRT.VRCQuestTools.Utils
{
    internal class CacheManager
    {
        internal static CacheManager Texture = new CacheManager(Path.Combine("Library", VRCQuestTools.Name, "TextureCache"));

        private string cachePath;

        public CacheManager(string cachePath)
        {
            this.cachePath = cachePath;
        }

        internal void Save(string fileName, string data)
        {
            Directory.CreateDirectory(cachePath);
            File.WriteAllText(Path.Combine(cachePath, fileName), data);
        }

        internal string LoadString(string fileName)
        {
            return File.ReadAllText(Path.Combine(cachePath, fileName));
        }

        internal void CopyFromCache(string fileName, string destPath)
        {
            File.Copy(Path.Combine(cachePath, fileName), destPath, true);
        }

        internal void CopyToCache(string srcPath, string fileName)
        {
            File.Copy(srcPath, Path.Combine(cachePath, fileName), true);
        }

        internal bool Exists(string fileName)
        {
            return File.Exists(Path.Combine(cachePath, fileName));
        }
    }
}
