using UnityEditor;
using UnityEngine;

namespace KRTQuestTools
{
    public class BlendShapesCopy : EditorWindow
    {
        SkinnedMeshRenderer source;
        SkinnedMeshRenderer target;

        [MenuItem(KRTQuestTools.RootMenu + "BlendShapes Copy", false, (int)MenuPriority.BlendShapesCopy)]
        static void Init()
        {
            var window = (BlendShapesCopy)GetWindow(typeof(BlendShapesCopy));
            window.Show();
        }

        [MenuItem("CONTEXT/SkinnedMeshRenderer/Copy BlendShape Weights")]
        static void InitOnContext(MenuCommand command)
        {
            var window = (BlendShapesCopy)GetWindow(typeof(BlendShapesCopy));
            window.source = (SkinnedMeshRenderer)command.context;
            window.Show();
        }

        private void OnGUI()
        {
            titleContent.text = "BlendShapes Copy";
            source = (SkinnedMeshRenderer)EditorGUILayout.ObjectField("Source mesh", source, typeof(SkinnedMeshRenderer), true);
            target = (SkinnedMeshRenderer)EditorGUILayout.ObjectField("Target mesh", target, typeof(SkinnedMeshRenderer), true);
            EditorGUILayout.Space();
            EditorGUI.BeginDisabledGroup(source == null || target == null);
            if (GUILayout.Button("Copy BlendShape Weights"))
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
            if (GUILayout.Button("Switch Source/Target"))
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
