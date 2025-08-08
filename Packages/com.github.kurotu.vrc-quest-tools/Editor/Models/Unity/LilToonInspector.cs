using KRT.VRCQuestTools.Utils;

namespace KRT.VRCQuestTools.Models.Unity
{
    /// <summary>
    /// lilToonInspector proxy.
    /// </summary>
    internal static class LilToonInspector
    {
        /// <summary>
        /// Gets the shader setting path for lilToon.
        /// </summary>
        /// <returns>File path to the shader settings.</returns>
        public static string GetShaderSettingPath()
        {
            // https://lilxyzw.github.io/lilToon/ja_JP/migrate1to2.html#liltooninspector
            if (new SemVer(2, 0, 0) <= AssetUtility.LilToonVersion)
            {
                var lilDirectoryManager = SystemUtility.GetTypeByName("lilToon.lilDirectoryManager");
                if (lilDirectoryManager == null)
                {
                    throw new LilToonCompatibilityException("lilToon found, but lilToon.lilDirectoryManager not found");
                }
                var method = lilDirectoryManager.GetMethod("GetShaderSettingPath");
                if (method == null)
                {
                    throw new LilToonCompatibilityException("lilToon.lilDirectoryManager.GetShaderSettingPath not found");
                }
                return method.Invoke(null, null) as string;
            }
            return Invoke<string>("GetShaderSettingPath");
        }

        private static T Invoke<T>(string name)
            where T : class
        {
            if (!AssetUtility.IsLilToonImported())
            {
                throw new LilToonCompatibilityException("lilToon not found in Assets");
            }
            var lilToonInspector = SystemUtility.GetTypeByName("lilToon.lilToonInspector");
            if (lilToonInspector == null)
            {
                throw new LilToonCompatibilityException("lilToon found, but lilToon.lilToonInspector not found");
            }
            var method = lilToonInspector.GetMethod(name);
            if (method == null)
            {
                throw new LilToonCompatibilityException($"{lilToonInspector.Name}.{name} not found");
            }
            return method.Invoke(null, null) as T;
        }
    }
}
