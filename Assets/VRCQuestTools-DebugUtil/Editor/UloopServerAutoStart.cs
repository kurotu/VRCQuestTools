using System.Threading.Tasks;
using io.github.hatayama.uLoopMCP;
using UnityEditor;

namespace KRT.VRCQuestTools.Debug
{
    /// <summary>
    /// Ensures the uLoopMCP server is running whenever the Unity Editor launches or reloads,
    /// as a safety net in case McpServerController's own session restoration does not start it
    /// (e.g. isServerRunning was left disabled in UserSettings/UnityMcpSettings.json).
    /// </summary>
    [InitializeOnLoad]
    internal static class UloopServerAutoStart
    {
        static UloopServerAutoStart()
        {
            EditorApplication.delayCall += () => _ = StartServerIfNeededAsync();
        }

        // McpServerController runs its own startup recovery on delayCall too. Awaiting
        // RecoveryTask first avoids a second concurrent StartServer() call racing with it
        // (StartServerWithUseCaseAsync stops any existing server before starting a new one,
        // so two overlapping calls can leave the server stopped instead of running).
        private static async Task StartServerIfNeededAsync()
        {
            Task recoveryTask = McpServerController.RecoveryTask;
            if (recoveryTask != null)
            {
                try
                {
                    await recoveryTask;
                }
                catch
                {
                    // Failure is already logged by McpServerController itself.
                }
            }

            if (!McpServerController.IsServerRunning)
            {
                McpServerController.StartServer();
            }
        }
    }
}
