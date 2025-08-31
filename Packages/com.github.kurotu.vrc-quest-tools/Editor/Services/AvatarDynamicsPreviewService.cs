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

            // Draw the PhysBone tree as capsules between connected transforms
            DrawPhysBoneTransformTree(provider, provider.RootTransform, 0f, 0);

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
            // Draw wireframe sphere using Handles with consistent angular spacing
            Handles.DrawWireArc(center, Vector3.up, Vector3.forward, 360f, radius);
            Handles.DrawWireArc(center, Vector3.forward, Vector3.up, 360f, radius);
            Handles.DrawWireArc(center, Vector3.right, Vector3.up, 360f, radius);
            
            // Add additional arcs for better sphere definition aligned with other wireframes
            Handles.DrawWireArc(center, (Vector3.forward + Vector3.right).normalized, Vector3.up, 360f, radius);
        }

        private static void DrawWireCapsule(Vector3 center, Quaternion rotation, float radius, float height)
        {
            var halfHeight = height * 0.5f;
            var topCenter = center + rotation * Vector3.up * (halfHeight - radius);
            var bottomCenter = center + rotation * Vector3.up * -(halfHeight - radius);

            // Draw spheres at top and bottom
            DrawWireSphere(topCenter, radius);
            DrawWireSphere(bottomCenter, radius);

            // Draw connecting lines aligned with the sphere wireframes (every 45 degrees)
            for (int i = 0; i < 8; i++)
            {
                var angle = i * 45f * Mathf.Deg2Rad;
                var forward = rotation * Vector3.forward;
                var right = rotation * Vector3.right;
                var offset = forward * Mathf.Cos(angle) + right * Mathf.Sin(angle);
                
                var connectionOffset = offset * radius;
                Handles.DrawLine(topCenter + connectionOffset, bottomCenter + connectionOffset);
            }
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

        private static void DrawPhysBoneTransformTree(VRCPhysBoneProviderBase provider, Transform transform, float parentNormalizedPosition, int depth)
        {
            if (transform == null || depth > 20) // Safety limit to prevent infinite recursion
                return;

            // Check if this transform should be included
            if (provider.IgnoreTransforms != null && provider.IgnoreTransforms.Contains(transform))
                return;

            // Get child transforms that should be included
            var validChildren = new List<Transform>();
            for (int i = 0; i < transform.childCount; i++)
            {
                var child = transform.GetChild(i);
                if (provider.IgnoreTransforms == null || !provider.IgnoreTransforms.Contains(child))
                {
                    validChildren.Add(child);
                }
            }

            // Handle MultiChildType
            var childrenToProcess = new List<Transform>();
            if (validChildren.Count > 0)
            {
                switch (provider.MultiChildType)
                {
                    case MultiChildType.Ignore:
                        // Only process the first child, and skip the first capsule if this is the root
                        if (validChildren.Count > 0)
                        {
                            childrenToProcess.Add(validChildren[0]);
                        }
                        break;
                    case MultiChildType.First:
                        // Process only the first child
                        if (validChildren.Count > 0)
                        {
                            childrenToProcess.Add(validChildren[0]);
                        }
                        break;
                    case MultiChildType.Average:
                        // Process all children
                        childrenToProcess.AddRange(validChildren);
                        break;
                }
            }

            // Draw capsules to children (leaf transforms don't have capsules)
            bool isRoot = depth == 0;
            bool skipFirstCapsule = isRoot && provider.MultiChildType == MultiChildType.Ignore;

            foreach (var child in childrenToProcess)
            {
                // Skip the first capsule when MultiChildType is Ignore and this is the root
                if (skipFirstCapsule)
                {
                    skipFirstCapsule = false; // Only skip the very first one
                }
                else
                {
                    DrawPhysBoneCapsule(provider, transform, child, parentNormalizedPosition, depth);
                }

                // Recursively process children
                var childNormalizedPosition = (float)(depth + 1) / 20f; // Approximate normalized position
                DrawPhysBoneTransformTree(provider, child, childNormalizedPosition, depth + 1);
            }
        }

        private static void DrawPhysBoneCapsule(VRCPhysBoneProviderBase provider, Transform startTransform, Transform endTransform, float startNormalizedPosition, int depth)
        {
            var startPos = startTransform.position;
            var endPos = endTransform.position;
            var distance = Vector3.Distance(startPos, endPos);

            if (distance <= 0.001f) // Skip if transforms are too close
                return;

            // Calculate normalized positions for radius curve evaluation
            var endNormalizedPosition = (float)(depth + 1) / 20f; // Approximate normalized position

            var startRadius = GetPhysBoneRadiusAtPosition(provider, startTransform, startNormalizedPosition);
            var endRadius = GetPhysBoneRadiusAtPosition(provider, endTransform, endNormalizedPosition);

            // Draw tapered capsule with different radii at each end
            DrawTaperedWireCapsule(startPos, endPos, startRadius, endRadius);
        }

        private static void DrawTaperedWireCapsule(Vector3 startPos, Vector3 endPos, float startRadius, float endRadius)
        {
            var direction = (endPos - startPos).normalized;
            var distance = Vector3.Distance(startPos, endPos);
            
            if (distance <= 0.001f)
                return;

            var rotation = Quaternion.FromToRotation(Vector3.up, direction);

            // Draw half spheres at each end (centered at transform positions)
            DrawWireHalfSphere(startPos, startRadius, rotation, true);  // Bottom half sphere
            DrawWireHalfSphere(endPos, endRadius, rotation, false);     // Top half sphere

            // Draw connecting lines for the truncated cone
            var perpendicular1 = Vector3.Cross(direction, Vector3.up).normalized;
            if (perpendicular1.magnitude < 0.1f) // Handle case where direction is parallel to up
            {
                perpendicular1 = Vector3.Cross(direction, Vector3.forward).normalized;
            }
            var perpendicular2 = Vector3.Cross(direction, perpendicular1).normalized;

            // Draw connecting lines aligned with the meridian arcs (every 45 degrees)
            for (int i = 0; i < 8; i++)
            {
                var angle = i * 45f * Mathf.Deg2Rad;
                var offset = perpendicular1 * Mathf.Cos(angle) + perpendicular2 * Mathf.Sin(angle);
                
                var startPoint = startPos + offset * startRadius;
                var endPoint = endPos + offset * endRadius;
                
                Handles.DrawLine(startPoint, endPoint);
            }
        }

        private static void DrawWireHalfSphere(Vector3 center, float radius, Quaternion rotation, bool isBottom)
        {
            // Draw hemisphere by drawing arcs
            var up = rotation * Vector3.up;
            var forward = rotation * Vector3.forward;
            var right = rotation * Vector3.right;

            // Draw the circular edge of the hemisphere
            Handles.DrawWireArc(center, up, forward, 360f, radius);

            // Draw meridian arcs (only the hemisphere part) - aligned with connecting lines
            var arcDirection = isBottom ? -up : up;
            
            // Draw 8 meridian arcs for the hemisphere (every 45 degrees to align with connecting lines)
            for (int i = 0; i < 8; i++)
            {
                var meridianDirection = Quaternion.AngleAxis(i * 45f, up) * forward;
                Handles.DrawWireArc(center, meridianDirection, arcDirection, 180f, radius);
            }
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
