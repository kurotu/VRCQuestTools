// <copyright file="BlendShapesCopy.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using UnityEditor;
using UnityEngine;

namespace KRT.VRCQuestTools
{
    public class BlendShapesCopy : EditorWindow
    {
        SkinnedMeshRenderer source;
        SkinnedMeshRenderer target;

        internal static void InitFromMenu()
        {
            var window = (BlendShapesCopy)GetWindow(typeof(BlendShapesCopy));
            window.Show();
        }

        [MenuItem(ContextMenu.ContextBlendShapesCopy)]
        static void InitOnContext(MenuCommand command)
        {
            var window = (BlendShapesCopy)GetWindow(typeof(BlendShapesCopy));
            window.source = (SkinnedMeshRenderer)command.context;
            window.Show();
        }

        private void OnGUI()
        {
            var i18n = VRCQuestToolsSettings.I18nResource;
            titleContent.text = "BlendShapes Copy";
            source = (SkinnedMeshRenderer)EditorGUILayout.ObjectField(i18n.SourceMeshLabel, source, typeof(SkinnedMeshRenderer), true);
            target = (SkinnedMeshRenderer)EditorGUILayout.ObjectField(i18n.TargetMeshLabel, target, typeof(SkinnedMeshRenderer), true);
            EditorGUILayout.Space();
            EditorGUI.BeginDisabledGroup(source == null || target == null);
            if (GUILayout.Button(i18n.CopyButtonLabel))
            {
                if (source.sharedMesh == null)
                {
                    EditorUtility.DisplayDialog("Warning - KRT Avatar Tools", "Source has no mesh", "OK");
                    return;
                }
                if (target.sharedMesh == null)
                {
                    EditorUtility.DisplayDialog("Warning - KRT Avatar Tools", "Target has no mesh", "OK");
                    return;
                }
                Undo.RecordObject(target, "Copy BlendShape Weights");
                CopyBlendShapeWeights(source, target);
            }
            EditorGUI.EndDisabledGroup();
            if (GUILayout.Button(i18n.SwitchButtonLabel))
            {
                var tmp = source;
                source = target;
                target = tmp;
            }
        }

        static void CopyBlendShapeWeights(SkinnedMeshRenderer source, SkinnedMeshRenderer target)
        {
            var count = source.sharedMesh.blendShapeCount;
            for (var i = 0; i < count; i++)
            {
                var name = source.sharedMesh.GetBlendShapeName(i);
                var weight = source.GetBlendShapeWeight(i);
                var targetIndex = target.sharedMesh.GetBlendShapeIndex(name);
                if (targetIndex < 0)
                {
                    continue;
                }
                target.SetBlendShapeWeight(targetIndex, weight);
            }
        }
    }
}
