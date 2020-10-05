using UnityEditor;
using UnityEngine;

namespace KRTQuestTools
{
    public class BlendShapesCopy : EditorWindow
    {
        readonly BlendShapesCopyI18nBase i18n = BlendShapesCopyI18n.Create();
        SkinnedMeshRenderer source;
        SkinnedMeshRenderer target;

        [MenuItem(MenuPaths.BlendShapesCopy, false, (int)MenuPriorities.BlendShapesCopy)]
        static void Init()
        {
            var window = (BlendShapesCopy)GetWindow(typeof(BlendShapesCopy));
            window.Show();
        }

        [MenuItem(MenuPaths.ContextBlendShapesCopy)]
        static void InitOnContext(MenuCommand command)
        {
            var window = (BlendShapesCopy)GetWindow(typeof(BlendShapesCopy));
            window.source = (SkinnedMeshRenderer)command.context;
            window.Show();
        }

        private void OnGUI()
        {
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

    static class BlendShapesCopyI18n
    {
        public static BlendShapesCopyI18nBase Create()
        {
            if (System.Globalization.CultureInfo.CurrentCulture.Name == "ja-JP")
            {
                return new BlendShapesCopyI18nJapanese();
            }
            else
            {
                return new BlendShapesCopyI18nEnglish();
            }
        }
    }

    abstract class BlendShapesCopyI18nBase
    {
        public abstract string SourceMeshLabel { get; }
        public abstract string TargetMeshLabel { get; }
        public abstract string CopyButtonLabel { get; }
        public abstract string SwitchButtonLabel { get; }
    }

    class BlendShapesCopyI18nEnglish : BlendShapesCopyI18nBase
    {
        public override string SourceMeshLabel => "Source Mesh";

        public override string TargetMeshLabel => "Target Mesh";

        public override string CopyButtonLabel => "Copy BlendShape Weights";

        public override string SwitchButtonLabel => "Switch Source/Target";
    }
    class BlendShapesCopyI18nJapanese : BlendShapesCopyI18nBase
    {
        public override string SourceMeshLabel => "コピー元メッシュ";

        public override string TargetMeshLabel => "コピー先メッシュ";

        public override string CopyButtonLabel => "ブレンドシェイプの値をコピー";

        public override string SwitchButtonLabel => "コピー元/コピー先を入れ替え";
    }
}
