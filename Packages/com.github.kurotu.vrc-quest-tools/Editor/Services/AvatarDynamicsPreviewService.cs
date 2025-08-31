// <copyright file="AvatarDynamicsPreviewService.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using System.Collections.Generic;
using System.Linq;
using KRT.VRCQuestTools.Models.VRChat;
using UnityEditor;
using UnityEngine;
using VRC.Dynamics;
using VRC.SDK3.Dynamics.PhysBone.Components;

namespace KRT.VRCQuestTools.Services
{
    /// <summary>
    /// Service for drawing Avatar Dynamics component previews in scene view.
    /// </summary>
    internal static class AvatarDynamicsPreviewService
    {
        private static IVRCAvatarDynamicsProvider hoveredProvider;
        private static readonly Color PhysBoneColor = Color.red;
        private static readonly Color ColliderColor = Color.blue;
        private static readonly Color ContactColor = Color.green;
        private static bool isInitialized = false;

        /// <summary>
        /// Sets the VRCPhysBoneProviderBase component to preview in the scene view.
        /// </summary>
        /// <param name="provider">Provider to preview, or null to clear preview.</param>
        internal static void SetPreviewComponent(IVRCAvatarDynamicsProvider provider)
        {
            if (hoveredProvider != provider)
            {
                hoveredProvider = provider;
                if (isInitialized)
                {
                    SceneView.RepaintAll();
                }
            }
        }

        /// <summary>
        /// Clears the current preview component.
        /// </summary>
        internal static void ClearPreview()
        {
            if (hoveredProvider != null)
            {
                hoveredProvider = null;
                if (isInitialized)
                {
                    SceneView.RepaintAll();
                }
            }
        }

        /// <summary>
        /// Initializes the preview service by subscribing to scene view events.
        /// </summary>
        internal static void Initialize()
        {
            if (!isInitialized)
            {
                SceneView.duringSceneGui -= OnSceneGUI;
                SceneView.duringSceneGui += OnSceneGUI;
                isInitialized = true;
            }
        }

        /// <summary>
        /// Cleanup the preview service by unsubscribing from scene view events.
        /// </summary>
        internal static void Cleanup()
        {
            SceneView.duringSceneGui -= OnSceneGUI;
            hoveredProvider = null;
            isInitialized = false;
        }

        private static void OnSceneGUI(SceneView sceneView)
        {
            if (hoveredProvider == null)
                return;

            var originalColor = Handles.color;

            try
            {
                switch (hoveredProvider.ComponentType)
                {
                    case AvatarDynamicsComponentType.PhysBone:
                        Handles.color = PhysBoneColor;
                        DrawPhysBonePreview((VRCPhysBoneProviderBase)hoveredProvider);
                        break;
                    case AvatarDynamicsComponentType.PhysBoneCollider:
                        Handles.color = ColliderColor;
                        DrawColliderPreview((VRCPhysBoneColliderProvider)hoveredProvider);
                        break;
                    case AvatarDynamicsComponentType.Contact:
                        Handles.color = ContactColor;
                        DrawContactPreview((VRCContactBaseProvider)hoveredProvider);
                        break;
                }
            }
            finally
            {
                Handles.color = originalColor;
            }
        }

        private static void DrawPhysBonePreview(VRCPhysBoneProviderBase provider)
        {
            if (provider == null || provider.RootTransform == null)
                return;

            // Draw the PhysBone chain as capsules between transforms
            var transforms = GetPhysBoneTransforms(provider);
            Transform previousTransform = null;
            float previousNormalizedPosition = 0f;

            for (int i = 0; i < transforms.Count; i++)
            {
                var transform = transforms[i];
                if (transform == null)
                    continue;

                // Calculate normalized position along the bone chain
                var normalizedPosition = (float)i / Mathf.Max(1f, transforms.Count - 1f);

                // Draw capsule between previous and current transform
                if (previousTransform != null)
                {
                    var startPos = previousTransform.position;
                    var endPos = transform.position;
                    var distance = Vector3.Distance(startPos, endPos);

                    if (distance > 0.001f) // Only draw if there's meaningful distance
                    {
                        var startRadius = GetPhysBoneRadiusAtPosition(provider, previousTransform, previousNormalizedPosition);
                        var endRadius = GetPhysBoneRadiusAtPosition(provider, transform, normalizedPosition);
                        var direction = (endPos - startPos).normalized;
                        var rotation = Quaternion.FromToRotation(Vector3.up, direction);

                        // Position capsule so edge spheres are centered at transform positions
                        // Capsule height should be distance + startRadius + endRadius
                        var capsuleHeight = distance + startRadius + endRadius;
                        var center = startPos + direction * (distance * 0.5f + startRadius);

                        // For now, use average radius for the capsule body
                        // In future, could implement tapered capsules
                        var avgRadius = (startRadius + endRadius) * 0.5f;

                        DrawWireCapsule(center, rotation, avgRadius, capsuleHeight);
                    }
                }

                previousTransform = transform;
                previousNormalizedPosition = normalizedPosition;
            }

            // Draw colliders referenced by this PhysBone
            var originalColor = Handles.color;
            Handles.color = ColliderColor;
            try
            {
                if (provider.Colliders != null)
                {
                    foreach (var colliderComponent in provider.Colliders)
                    {
                        if (colliderComponent is VRCPhysBoneCollider collider)
                        {
                            var colliderProvider = new VRCPhysBoneColliderProvider(collider);
                            DrawColliderPreview(colliderProvider);
                        }
                    }
                }
            }
            finally
            {
                Handles.color = originalColor;
            }
        }

        private static void DrawColliderPreview(VRCPhysBoneColliderProvider provider)
        {
            if (provider?.Component == null)
                return;

            var collider = provider.Component as VRCPhysBoneCollider;
            var transform = provider.Component.transform;

            // Apply world transform with collider's local position and rotation offsets
            var worldMatrix = transform.localToWorldMatrix;
            var localPosition = collider.position;
            var localRotation = collider.rotation;

            var worldPosition = worldMatrix.MultiplyPoint3x4(localPosition);
            var worldRotation = transform.rotation * localRotation;

            switch (provider.ShapeType)
            {
                case VRCPhysBoneCollider.ShapeType.Sphere:
                    var sphereRadius = provider.Radius * Mathf.Max(transform.lossyScale.x, transform.lossyScale.y, transform.lossyScale.z);
                    DrawWireSphere(worldPosition, sphereRadius);
                    break;

                case VRCPhysBoneCollider.ShapeType.Capsule:
                    var capsuleRadius = provider.Radius * Mathf.Max(transform.lossyScale.x, transform.lossyScale.z);
                    var capsuleHeight = provider.Height * transform.lossyScale.y;
                    DrawWireCapsule(worldPosition, worldRotation, capsuleRadius, capsuleHeight);
                    break;

                case VRCPhysBoneCollider.ShapeType.Plane:
                    var planeScale = Mathf.Max(transform.lossyScale.x, transform.lossyScale.z) * 2f;
                    DrawWirePlane(worldPosition, worldRotation, planeScale);
                    break;
            }
        }

        private static void DrawContactPreview(VRCContactBaseProvider provider)
        {
            if (provider?.Component == null)
                return;

            var contact = provider.Component as ContactBase;
            var transform = provider.Component.transform;

            // Apply world transform with contact's local position offset if it exists
            var worldMatrix = transform.localToWorldMatrix;
            var localPosition = contact.position;

            var worldPosition = worldMatrix.MultiplyPoint3x4(localPosition);
            var radius = provider.Radius * Mathf.Max(transform.lossyScale.x, transform.lossyScale.y, transform.lossyScale.z);
            DrawWireSphere(worldPosition, radius);
        }

        private static void DrawWireSphere(Vector3 center, float radius)
        {
            // Draw wireframe sphere using Handles
            Handles.DrawWireArc(center, Vector3.up, Vector3.forward, 360f, radius);
            Handles.DrawWireArc(center, Vector3.forward, Vector3.up, 360f, radius);
            Handles.DrawWireArc(center, Vector3.right, Vector3.up, 360f, radius);
        }

        private static void DrawWireCapsule(Vector3 center, Quaternion rotation, float radius, float height)
        {
            var halfHeight = height * 0.5f;
            var topCenter = center + rotation * Vector3.up * (halfHeight - radius);
            var bottomCenter = center + rotation * Vector3.up * -(halfHeight - radius);

            // Draw spheres at top and bottom
            DrawWireSphere(topCenter, radius);
            DrawWireSphere(bottomCenter, radius);

            // Draw connecting lines
            var forward = rotation * Vector3.forward * radius;
            var right = rotation * Vector3.right * radius;
            var back = rotation * Vector3.back * radius;
            var left = rotation * Vector3.left * radius;

            Handles.DrawLine(topCenter + forward, bottomCenter + forward);
            Handles.DrawLine(topCenter + right, bottomCenter + right);
            Handles.DrawLine(topCenter + back, bottomCenter + back);
            Handles.DrawLine(topCenter + left, bottomCenter + left);
        }

        private static void DrawWirePlane(Vector3 center, Quaternion rotation, float size)
        {
            var halfSize = size * 0.5f;
            var corners = new Vector3[]
            {
                center + rotation * new Vector3(-halfSize, 0, -halfSize),
                center + rotation * new Vector3(halfSize, 0, -halfSize),
                center + rotation * new Vector3(halfSize, 0, halfSize),
                center + rotation * new Vector3(-halfSize, 0, halfSize)
            };

            // Draw rectangle
            for (int i = 0; i < corners.Length; i++)
            {
                var nextIndex = (i + 1) % corners.Length;
                Handles.DrawLine(corners[i], corners[nextIndex]);
            }

            // Draw diagonals
            Handles.DrawLine(corners[0], corners[2]);
            Handles.DrawLine(corners[1], corners[3]);
        }

        private static List<Transform> GetPhysBoneTransforms(VRCPhysBoneProviderBase provider)
        {
            var transforms = new List<Transform>();
            var rootTransform = provider.RootTransform;

            if (rootTransform == null)
                return transforms;

            // Add root transform
            transforms.Add(rootTransform);

            // Get all child transforms based on the PhysBone configuration
            AddChildTransforms(rootTransform, transforms, provider);

            return transforms;
        }

        private static void AddChildTransforms(Transform parent, List<Transform> transforms, VRCPhysBoneProviderBase provider)
        {
            for (int i = 0; i < parent.childCount; i++)
            {
                var child = parent.GetChild(i);

                // Check if this child should be included based on PhysBone settings
                if (ShouldIncludeTransform(child, provider))
                {
                    transforms.Add(child);

                    // Recursively add children if not at max depth
                    if (transforms.Count < 100) // Safety limit to prevent infinite recursion
                    {
                        AddChildTransforms(child, transforms, provider);
                    }
                }
            }
        }

        private static bool ShouldIncludeTransform(Transform transform, VRCPhysBoneProviderBase provider)
        {
            // Basic check - include if not in ignore list
            if (provider.IgnoreTransforms != null && provider.IgnoreTransforms.Contains(transform))
                return false;

            // Include if has significant child count or is explicitly included
            return true;
        }

        private static float GetPhysBoneRadiusAtPosition(VRCPhysBoneProviderBase provider, Transform transform, float normalizedPosition)
        {
            // Use the abstraction layer radius
            var radius = provider.Radius;

            // Apply radius curve if available
            if (provider.RadiusCurve != null && provider.RadiusCurve.keys.Length > 0)
            {
                radius *= provider.RadiusCurve.Evaluate(normalizedPosition);
            }

            return radius * Mathf.Max(transform.lossyScale.x, transform.lossyScale.y, transform.lossyScale.z);
        }
    }
}
