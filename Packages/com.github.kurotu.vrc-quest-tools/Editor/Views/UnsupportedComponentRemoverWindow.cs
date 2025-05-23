using System.Collections.Generic;
using System.Linq;
using KRT.VRCQuestTools.Models;
using KRT.VRCQuestTools.Utils;
using UnityEditor;
using UnityEngine;

namespace KRT.VRCQuestTools.Views
{
    /// <summary>
    /// Editor window to remove unsupported components.
    /// </summary>
    internal class UnsupportedComponentRemoverWindow : EditorWindow
    {
        [SerializeField]
        private GameObject gameObject;

        [SerializeField]
        private Vector2 scrollPosition;

        /// <summary>
        /// Show UnsupportedComponentRemoverWindow with selected GameObject.
        /// </summary>
        /// <param name="gameObject">GameObject to show.</param>
        internal static void Show(GameObject gameObject)
        {
            var window = GetWindow<UnsupportedComponentRemoverWindow>();
            window.gameObject = gameObject;
            window.Show();
        }

        private void OnEnable()
        {
            titleContent = new GUIContent("Unsupported Components");
        }

        private void OnGUI()
        {
            var i18n = VRCQuestToolsSettings.I18nResource;
            gameObject = (GameObject)EditorGUILayout.ObjectField(gameObject, typeof(GameObject), true);

            if (gameObject == null)
            {
                return;
            }

            var components = VRCQuestTools.ComponentRemover.GetUnsupportedComponentsInChildren(gameObject, true);
#if VQT_HAS_VRCSDK_NO_PRECHECK && VQT_HAS_MA_CONVERT_CONSTRAINTS
            if (gameObject.GetComponent<nadena.dev.modular_avatar.core.ModularAvatarConvertConstraints>() != null)
            {
                components = components.Where(c => !(c is UnityEngine.Animations.IConstraint)).ToArray();
            }
#endif
            var maComponents = ModularAvatarUtility.GetUnsupportedComponentsInChildren(gameObject, true);

            using (var scrollView = new EditorGUILayout.ScrollViewScope(scrollPosition))
            {
                scrollPosition = scrollView.scrollPosition;

                if (components.Length > 0 || maComponents.Length > 0)
                {
                    EditorGUILayout.LabelField(i18n.UnsupportedRemoverConfirmationMessage(gameObject.name), EditorStyles.wordWrappedLabel);
                }
                else
                {
                    EditorGUILayout.LabelField(i18n.NoUnsupportedComponentsMessage(gameObject.name), EditorStyles.wordWrappedLabel);
                }
                EditorGUILayout.Space();

                if (components.Length > 0)
                {
                    using (new EditorGUI.DisabledScope(true))
                    {
                        foreach (var item in components)
                        {
                            EditorGUILayout.ObjectField(item, item.GetType(), true);
                        }
                    }
                    EditorGUILayout.Space();
                }

                if (maComponents.Length > 0)
                {
                    using (new EditorGUI.DisabledScope(true))
                    {
                        foreach (var item in maComponents)
                        {
                            EditorGUILayout.ObjectField(item, item.GetType(), true);
                        }
                    }
                    EditorGUILayout.Space();
                }
            }

            using (new EditorGUI.DisabledScope(components.Length == 0 && maComponents.Length == 0))
            {
                if (GUILayout.Button(i18n.RemoveLabel))
                {
                    Undo.SetCurrentGroupName("Remove Unsupported Components");

                    var allowedComponents = new List<System.Type>();
#if VQT_HAS_VRCSDK_NO_PRECHECK && VQT_HAS_MA_CONVERT_CONSTRAINTS
                    if (gameObject.GetComponent<nadena.dev.modular_avatar.core.ModularAvatarConvertConstraints>() != null)
                    {
                        allowedComponents.Add(typeof(UnityEngine.Animations.IConstraint));
                        VRCQuestTools.ComponentRemover.RemoveUnsupportedComponentsInChildren(gameObject.gameObject, true, true, new System.Type[] { typeof(UnityEngine.Animations.IConstraint) });
                    }
#endif
                    VRCQuestTools.ComponentRemover.RemoveUnsupportedComponentsInChildren(gameObject.gameObject, true, true, allowedComponents.ToArray());
                    ModularAvatarUtility.RemoveUnsupportedComponents(gameObject.gameObject, true);
                    Close();
                }
            }
        }
    }
}
