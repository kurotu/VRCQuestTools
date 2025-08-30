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
        private static Component hoveredComponent;
        private static readonly Color PhysBoneColor = Color.red;
        private static readonly Color ColliderColor = Color.blue;
        private static readonly Color ContactColor = Color.green;
        private static bool isInitialized = false;

        /// <summary>
        /// Sets the VRCPhysBoneProviderBase component to preview in the scene view.
        /// </summary>
        /// <param name="provider">Provider to preview, or null to clear preview.</param>
        internal static void SetPreviewComponent(VRCPhysBoneProviderBase provider)
        {
            var component = provider?.Component;
            if (hoveredComponent != component)
            {
                hoveredComponent = component;
                if (isInitialized)
                {
                    SceneView.RepaintAll();
                }
            }
        }

        /// <summary>
        /// Sets the PhysBoneCollider component to preview in the scene view.
        /// </summary>
        /// <param name="collider">Collider to preview, or null to clear preview.</param>
        internal static void SetPreviewComponent(VRCPhysBoneCollider collider)
        {
            if (hoveredComponent != collider)
            {
                hoveredComponent = collider;
                if (isInitialized)
                {
                    SceneView.RepaintAll();
                }
            }
        }

        /// <summary>
        /// Sets the ContactBase component to preview in the scene view.
        /// </summary>
        /// <param name="contact">Contact to preview, or null to clear preview.</param>
        internal static void SetPreviewComponent(ContactBase contact)
        {
            if (hoveredComponent != contact)
            {
                hoveredComponent = contact;
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
            if (hoveredComponent != null)
            {
                hoveredComponent = null;
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
            hoveredComponent = null;
            isInitialized = false;
        }

        private static void OnSceneGUI(SceneView sceneView)
        {
            if (hoveredComponent == null)
                return;

            var originalColor = Handles.color;

            try
            {
                // Handle VRCPhysBone through abstraction
                if (hoveredComponent is VRCPhysBone physBone)
                {
                    var provider = new VRCPhysBoneProvider(physBone);
                    Handles.color = PhysBoneColor;
                    DrawPhysBonePreview(provider);
                }
                else if (hoveredComponent is VRCPhysBoneCollider collider)
                {
                    Handles.color = ColliderColor;
                    DrawColliderPreview(collider);
                }
                else if (hoveredComponent is ContactBase contact)
                {
                    Handles.color = ContactColor;
                    DrawContactPreview(contact);
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

            // Draw the PhysBone chain
            var transforms = GetPhysBoneTransforms(provider);
            var previousTransform = provider.RootTransform;

            foreach (var transform in transforms)
            {
                if (transform == null)
                    continue;

                // Draw wireframe sphere at each transform
                var radius = GetPhysBoneRadius(provider, transform);
                DrawWireSphere(transform.position, radius);

                // Draw line connecting to previous transform
                if (previousTransform != null && previousTransform != transform)
                {
                    Handles.DrawLine(previousTransform.position, transform.position);
                }

                previousTransform = transform;
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
                            DrawColliderPreview(collider);
                        }
                    }
                }
            }
            finally
            {
                Handles.color = originalColor;
            }
        }

        private static void DrawColliderPreview(VRCPhysBoneCollider collider)
        {
            if (collider == null)
                return;

            var transform = collider.transform;
            var position = transform.position;
            var rotation = transform.rotation;

            switch (collider.shapeType)
            {
                case VRCPhysBoneCollider.ShapeType.Sphere:
                    var sphereRadius = collider.radius * Mathf.Max(transform.lossyScale.x, transform.lossyScale.y, transform.lossyScale.z);
                    DrawWireSphere(position, sphereRadius);
                    break;

                case VRCPhysBoneCollider.ShapeType.Capsule:
                    var capsuleRadius = collider.radius * Mathf.Max(transform.lossyScale.x, transform.lossyScale.z);
                    var capsuleHeight = collider.height * transform.lossyScale.y;
                    DrawWireCapsule(position, rotation, capsuleRadius, capsuleHeight);
                    break;

                case VRCPhysBoneCollider.ShapeType.Plane:
                    var planeScale = Mathf.Max(transform.lossyScale.x, transform.lossyScale.z) * 2f;
                    DrawWirePlane(position, rotation, planeScale);
                    break;
            }
        }

        private static void DrawContactPreview(ContactBase contact)
        {
            if (contact == null)
                return;

            var position = contact.transform.position;
            var radius = contact.radius * Mathf.Max(contact.transform.lossyScale.x, contact.transform.lossyScale.y, contact.transform.lossyScale.z);
            DrawWireSphere(position, radius);
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

        private static float GetPhysBoneRadius(VRCPhysBoneProviderBase provider, Transform transform)
        {
            // Use the abstraction layer radius
            var radius = provider.Radius;
            
            // Apply any radius curve if available
            if (provider.RadiusCurve != null && provider.RadiusCurve.keys.Length > 0)
            {
                // Calculate position along the bone chain (simplified)
                var normalizedPosition = 0.5f; // This would need proper calculation
                radius *= provider.RadiusCurve.Evaluate(normalizedPosition);
            }

            return radius * Mathf.Max(transform.lossyScale.x, transform.lossyScale.y, transform.lossyScale.z);
        }
    }
}