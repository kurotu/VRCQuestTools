using System;
using KRT.VRCQuestTools.Inspector;

namespace KRT.VRCQuestTools.Debug.Screenshots
{
    /// <summary>
    /// Forces the AvatarConverterSettings Inspector's "Avatar Dynamics Settings" foldout open for
    /// the duration of the scope, restoring the developer's original foldout state on Dispose.
    /// That state lives in a <c>ScriptableSingleton</c> (<see cref="AvatarConverterSettingsEditorState"/>)
    /// persisted across the whole Editor session, so without saving/restoring it, running the
    /// screenshot tool would permanently change the foldout state of every real
    /// AvatarConverterSettings Inspector the developer opens afterward.
    /// </summary>
    /// <remarks>
    /// "Material Conversion Settings" is deliberately left alone: the default "Toon Standard"
    /// conversion settings it reveals are themselves several nested foldouts deep (mask/MatCap
    /// texture settings, fallback shading, features, ...), so forcing it open would require an
    /// impractically tall screenshot just to reach the "Additional Material Conversion Settings"
    /// sample entry underneath it.
    /// </remarks>
    internal sealed class AvatarConverterSettingsFoldoutScope : IDisposable
    {
        private readonly bool originalFoldOutAvatarDynamics;
        private bool disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="AvatarConverterSettingsFoldoutScope"/> class.
        /// </summary>
        internal AvatarConverterSettingsFoldoutScope()
        {
            var state = AvatarConverterSettingsEditorState.instance;
            originalFoldOutAvatarDynamics = state.foldOutAvatarDynamics;
            state.foldOutAvatarDynamics = true;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (disposed)
            {
                return;
            }

            disposed = true;
            AvatarConverterSettingsEditorState.instance.foldOutAvatarDynamics = originalFoldOutAvatarDynamics;
        }
    }
}
