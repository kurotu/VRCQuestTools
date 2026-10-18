// <copyright file="BuildTargetDrawer.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using BuildTarget = KRT.VRCQuestTools.Models.BuildTarget;

namespace KRT.VRCQuestTools.Inspector
{
    /// <summary>
    /// PropertyDrawer for BuildTarget.
    /// </summary>
    [CustomPropertyDrawer(typeof(BuildTarget))]
    internal class BuildTargetDrawer : PropertyDrawer
    {
        private static readonly GUIContent[] Labels = Enum.GetValues(typeof(BuildTarget))
            .Cast<BuildTarget>()
            .Select(t => new GUIContent(t == BuildTarget.Android ? "Mobile" : t.ToString()))
            .ToArray();

        /// <inheritdoc/>
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            using (new EditorGUI.PropertyScope(position, label, property))
            {
                using (var ccs = new EditorGUI.ChangeCheckScope())
                {
                    var newIndex = EditorGUI.Popup(position, label, property.enumValueIndex, Labels);
                    if (ccs.changed)
                    {
                        property.enumValueIndex = newIndex;
                    }
                }
            }
        }
    }
}
