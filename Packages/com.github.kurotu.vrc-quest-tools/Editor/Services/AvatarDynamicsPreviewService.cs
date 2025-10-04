// <copyright file="AvatarDynamicsPreviewService.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using KRT.VRCQuestTools.Models.VRChat;
using UnityEditor;
using UnityEngine;
using VRC.Dynamics;
using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Dynamics.PhysBone.Components;

namespace KRT.VRCQuestTools.Services
{
    /// <summary>
    /// Service for drawing Avatar Dynamics component previews in scene view.
    /// </summary>
    internal static class AvatarDynamicsPreviewService
    {
        private const int MaxHierarchyDepth = 20;
        private const float MinimumSegmentLength = 0.001f;
        private const float MinimumDirectionMagnitude = 1e-6f;

        private static readonly Color PrimaryPreviewColor = Color.magenta;
        private static readonly Color SecondaryPreviewColor = Color.blue;

        private static IVRCAvatarDynamicsProvider hoveredProvider;
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
            {
                return;
            }

            var originalColor = Handles.color;

            try
            {
                switch (hoveredProvider.ComponentType)
                {
                    case AvatarDynamicsComponentType.PhysBone:
                        Handles.color = PrimaryPreviewColor;
                        DrawPhysBonePreview((VRCPhysBoneProviderBase)hoveredProvider);
                        break;
                    case AvatarDynamicsComponentType.PhysBoneCollider:
                        Handles.color = PrimaryPreviewColor;
                        DrawColliderPreview((VRCPhysBoneColliderProvider)hoveredProvider, drawRelatedPhysBones: true);
                        break;
                    case AvatarDynamicsComponentType.Contact:
                        Handles.color = PrimaryPreviewColor;
                        DrawContactPreview((VRCContactBaseProvider)hoveredProvider);
                        break;
                }
            }
            finally
            {
                Handles.color = originalColor;
            }
        }

        private static void DrawPhysBonePreview(VRCPhysBoneProviderBase provider, bool includeReferencedColliders = true)
        {
            if (provider?.RootTransform == null)
            {
                return;
            }

            var graph = BuildPhysBoneGraph(provider);
            DrawPhysBoneTransformTree(provider, provider.RootTransform, graph, depth: 0);

            if (!includeReferencedColliders || provider.Colliders == null)
            {
                return;
            }

            using var colorScope = new HandlesColorScope(SecondaryPreviewColor);
            foreach (var colliderComponent in provider.Colliders)
            {
                if (colliderComponent is VRCPhysBoneCollider collider)
                {
                    DrawColliderPreview(new VRCPhysBoneColliderProvider(collider));
                }
            }
        }

        private static void DrawColliderPreview(VRCPhysBoneColliderProvider provider, bool drawRelatedPhysBones = false)
        {
            if (provider?.Component is not VRCPhysBoneCollider collider)
            {
                return;
            }

            var transform = collider.transform;
            var worldPosition = transform.TransformPoint(collider.position);
            var worldRotation = transform.rotation * collider.rotation;

            DrawColliderShape(provider, worldPosition, worldRotation, transform);

            if (!drawRelatedPhysBones)
            {
                return;
            }

            using var colorScope = new HandlesColorScope(SecondaryPreviewColor);
            foreach (var physBoneProvider in GetPhysBoneProvidersReferencingCollider(collider))
            {
                DrawPhysBonePreview(physBoneProvider, includeReferencedColliders: false);
            }
        }

        private static void DrawColliderShape(VRCPhysBoneColliderProvider provider, Vector3 worldPosition, Quaternion worldRotation, Transform transform)
        {
            const float planePreviewScreenSize = 1.0f;

            switch (provider.ShapeType)
            {
                case VRCPhysBoneCollider.ShapeType.Sphere:
                    DrawWireSphere(worldPosition, provider.Radius * GetMaxLossyScale(transform));
                    break;

                case VRCPhysBoneCollider.ShapeType.Capsule:
                    DrawWireCapsule(
                        worldPosition,
                        worldRotation,
                        provider.Radius * Mathf.Max(transform.lossyScale.x, transform.lossyScale.z),
                        provider.Height * transform.lossyScale.y);
                    break;

                case VRCPhysBoneCollider.ShapeType.Plane:
                    DrawWirePlane(worldPosition, worldRotation, planePreviewScreenSize);
                    break;

                default:
                    break;
            }
        }

        private static IEnumerable<VRCPhysBoneProviderBase> GetPhysBoneProvidersReferencingCollider(VRCPhysBoneCollider collider)
        {
            if (collider == null)
            {
                yield break;
            }

            var visited = new HashSet<VRCPhysBone>();
            foreach (var physBone in EnumeratePhysBones(collider))
            {
                if (physBone == null || !visited.Add(physBone))
                {
                    continue;
                }

                if (ReferencesCollider(physBone, collider))
                {
                    yield return new VRCPhysBoneProvider(physBone);
                }
            }
        }

        private static void DrawContactPreview(VRCContactBaseProvider provider)
        {
            if (provider?.Component is not ContactBase contact)
            {
                return;
            }

            var transform = contact.transform;
            var worldPosition = transform.localToWorldMatrix.MultiplyPoint3x4(contact.position);
            var radius = provider.Radius * GetMaxLossyScale(transform);
            DrawWireSphere(worldPosition, radius);
        }

        private static void DrawWireSphere(Vector3 center, float radius)
        {
            // Draw wireframe sphere using Handles with consistent angular spacing
            Handles.DrawWireArc(center, Vector3.up, Vector3.forward, 360f, radius);
            Handles.DrawWireArc(center, Vector3.forward, Vector3.up, 360f, radius);
            Handles.DrawWireArc(center, Vector3.right, Vector3.up, 360f, radius);

            // Keep three orthogonal arcs to avoid redundant lines in the sphere visualization
        }

        private static void DrawWireCapsule(Vector3 center, Quaternion rotation, float radius, float height)
        {
            var up = rotation * Vector3.up;
            var right = rotation * Vector3.right;
            var forward = rotation * Vector3.forward;

            var halfHeight = height * 0.5f;
            var seamOffset = halfHeight - radius;
            if (seamOffset < 0f)
            {
                seamOffset = 0f; // guard: avoid negative when height < 2r
            }

            var topCenter = center + up * seamOffset;
            var bottomCenter = center - up * seamOffset;

            // Oval 1: in the Up-Forward plane (axis normal = Right)
            //   Ensure the top arc passes through +up and the bottom arc through -up
            var fromTop_UF = -Vector3.Cross(right, up).normalized;      // endpoints at +/- forward
            var fromBottom_UF = -Vector3.Cross(right, -up).normalized;  // endpoints at +/- forward
            Handles.DrawWireArc(topCenter, right, fromTop_UF, 180f, radius);
            Handles.DrawWireArc(bottomCenter, right, fromBottom_UF, 180f, radius);
            var sideA_TopUF = topCenter + forward * radius;
            var sideA_BotUF = bottomCenter + forward * radius;
            var sideB_TopUF = topCenter - forward * radius;
            var sideB_BotUF = bottomCenter - forward * radius;
            Handles.DrawLine(sideA_TopUF, sideA_BotUF);
            Handles.DrawLine(sideB_TopUF, sideB_BotUF);

            // Oval 2: in the Up-Right plane (axis normal = Forward)
            var fromTop_UR = -Vector3.Cross(forward, up).normalized;      // endpoints at +/- right
            var fromBottom_UR = -Vector3.Cross(forward, -up).normalized;  // endpoints at +/- right
            Handles.DrawWireArc(topCenter, forward, fromTop_UR, 180f, radius);
            Handles.DrawWireArc(bottomCenter, forward, fromBottom_UR, 180f, radius);
            var sideA_TopUR = topCenter + right * radius;
            var sideA_BotUR = bottomCenter + right * radius;
            var sideB_TopUR = topCenter - right * radius;
            var sideB_BotUR = bottomCenter - right * radius;
            Handles.DrawLine(sideA_TopUR, sideA_BotUR);
            Handles.DrawLine(sideB_TopUR, sideB_BotUR);

            // Seam circles: connect the junctions between semicircles and straight segments
            // Two rings around the capsule at the seam positions (normals = Up)
            Handles.DrawWireArc(topCenter, up, right, 360f, radius);
            Handles.DrawWireArc(bottomCenter, up, right, 360f, radius);
        }

        private static void DrawWirePlane(Vector3 center, Quaternion rotation, float screenSize)
        {
            var pixelConstantSize = Mathf.Max(HandleUtility.GetHandleSize(center) * screenSize, 0.001f);
            var halfSize = pixelConstantSize;
            var corners = new Vector3[]
            {
                center + rotation * new Vector3(-halfSize, 0, -halfSize),
                center + rotation * new Vector3(halfSize, 0, -halfSize),
                center + rotation * new Vector3(halfSize, 0, halfSize),
                center + rotation * new Vector3(-halfSize, 0, halfSize),
            };

            // Draw rectangle
            for (int i = 0; i < corners.Length; i++)
            {
                var nextIndex = (i + 1) % corners.Length;
                Handles.DrawLine(corners[i], corners[nextIndex]);
            }

            // Draw normal direction line from the center of the rectangle.
            var normalDirection = rotation * Vector3.up;
            var normalLength = pixelConstantSize;
            Handles.DrawLine(center, center + normalDirection * normalLength);
        }

        private static void DrawPhysBoneTransformTree(VRCPhysBoneProviderBase provider, Transform transform, PhysBoneGraph graph, int depth)
        {
            if (transform == null || depth > MaxHierarchyDepth || IsIgnoredTransform(provider, transform))
            {
                return;
            }

            var validChildren = GetValidChildren(provider, transform);
            var childrenToProcess = GetChildrenToProcess(provider, transform, depth, validChildren);
            var skipFirstCapsule = ShouldSkipFirstCapsule(provider, depth, validChildren.Count);

            foreach (var child in childrenToProcess)
            {
                if (skipFirstCapsule)
                {
                    skipFirstCapsule = false;
                }
                else
                {
                    DrawPhysBoneCapsule(provider, transform, child, graph);
                }

                DrawPhysBoneTransformTree(provider, child, graph, depth + 1);
            }

            if (childrenToProcess.Count == 0)
            {
                DrawEndpointExtension(provider, transform, graph);
            }
        }

        private static void DrawPhysBoneSegmentLine(Vector3 startPos, Vector3 endPos, Color baseColor)
        {
            if ((endPos - startPos).sqrMagnitude <= MinimumSegmentLength * MinimumSegmentLength)
            {
                return;
            }

            using var colorScope = new HandlesColorScope(Color.Lerp(baseColor, Color.white, 0.35f));
            Handles.DrawLine(startPos, endPos);
        }

        private static void DrawPhysBoneCapsule(VRCPhysBoneProviderBase provider, Transform startTransform, Transform endTransform, PhysBoneGraph graph)
        {
            var startPos = startTransform.position;
            var endPos = endTransform.position;
            var distance = Vector3.Distance(startPos, endPos);

            if (distance <= MinimumSegmentLength)
            {
                return;
            }

            DrawPhysBoneSegmentLine(startPos, endPos, Handles.color);

            var startNormalizedPosition = GetNormalizedDistance(graph, startTransform);
            var endNormalizedPosition = GetNormalizedDistance(graph, endTransform);

            var startRadius = GetPhysBoneRadiusAtPosition(provider, startTransform, startNormalizedPosition);
            var endRadius = GetPhysBoneRadiusAtPosition(provider, endTransform, endNormalizedPosition);

            if (startRadius > Mathf.Epsilon && endRadius > Mathf.Epsilon)
            {
                DrawTaperedWireCapsule(startPos, endPos, startTransform.rotation, startRadius, endRadius);
            }
        }

        private static void DrawTaperedWireCapsule(Vector3 startPos, Vector3 endPos, Quaternion referenceRotation, float startRadius, float endRadius)
        {
            var segment = endPos - startPos;
            var distance = segment.magnitude;

            if (distance <= MinimumSegmentLength)
            {
                return;
            }

            var up = segment / distance;
            var right = GetPerpendicularAxis(up, referenceRotation * Vector3.right, referenceRotation * Vector3.forward);
            var forward = Vector3.Cross(up, right).normalized;

            // Oval in the Up-Forward plane (axis normal = Right)
            var fromTopUF = forward;
            var fromBottomUF = -forward;
            Handles.DrawWireArc(endPos, right, fromTopUF, 180f, endRadius);
            Handles.DrawWireArc(startPos, right, fromBottomUF, 180f, startRadius);
            var sideATopUF = endPos + forward * endRadius;
            var sideABotUF = startPos + forward * startRadius;
            var sideBTopUF = endPos - forward * endRadius;
            var sideBBotUF = startPos - forward * startRadius;
            Handles.DrawLine(sideATopUF, sideABotUF);
            Handles.DrawLine(sideBTopUF, sideBBotUF);

            // Oval in the Up-Right plane (axis normal = Forward)
            Handles.DrawWireArc(endPos, forward, Vector3.Cross(up, forward).normalized, 180f, endRadius);
            Handles.DrawWireArc(startPos, forward, Vector3.Cross(-up, forward).normalized, 180f, startRadius);
            var sideATopUR = endPos + right * endRadius;
            var sideABotUR = startPos + right * startRadius;
            var sideBTopUR = endPos - right * endRadius;
            var sideBBotUR = startPos - right * startRadius;
            Handles.DrawLine(sideATopUR, sideABotUR);
            Handles.DrawLine(sideBTopUR, sideBBotUR);

            // End-cap circles around the start and end centers (normals = Up/Down along the bone)
            Handles.DrawWireArc(endPos, up, right, 360f, endRadius);
            Handles.DrawWireArc(startPos, up, right, 360f, startRadius);
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

            return radius * GetMaxLossyScale(transform);
        }

        private static float GetDistanceFromRoot(PhysBoneGraph graph, Transform t)
        {
            return graph.DistanceFromRoot.TryGetValue(t, out var d) ? d : 0f;
        }

        private static PhysBoneGraph BuildPhysBoneGraph(VRCPhysBoneProviderBase provider)
        {
            var graph = new PhysBoneGraph();

            // Compute endpoint length in world space once
            var worldOffset = provider.RootTransform != null
                ? provider.RootTransform.TransformVector(provider.EndpointPosition)
                : provider.EndpointPosition;
            graph.EndpointWorldLength = worldOffset.magnitude;

            if (provider.RootTransform == null)
            {
                return graph;
            }

            // DFS to accumulate distances
            void DFS(Transform current, int depth)
            {
                var validChildren = GetValidChildren(provider, current);
                var childrenToProcess = GetChildrenToProcess(provider, current, depth, validChildren);

                if (childrenToProcess.Count == 0)
                {
                    // Leaf: candidate for total length (include endpoint extension if any)
                    var d = GetDistanceFromRoot(graph, current);
                    if (d > graph.BoneLength)
                    {
                        graph.BoneLength = d;
                    }
                    var candidate = d + graph.EndpointWorldLength;
                    if (candidate > graph.TotalLength)
                    {
                        graph.TotalLength = candidate;
                    }
                }
                else
                {
                    foreach (var child in childrenToProcess)
                    {
                        // Distance from root for this child
                        var seg = Vector3.Distance(current.position, child.position);
                        var parentDist = GetDistanceFromRoot(graph, current);
                        var childDist = parentDist + seg;
                        if (!graph.DistanceFromRoot.ContainsKey(child) || graph.DistanceFromRoot[child] < childDist)
                        {
                            graph.DistanceFromRoot[child] = childDist;
                        }

                        DFS(child, depth + 1);
                    }
                }
            }

            graph.DistanceFromRoot[provider.RootTransform] = 0f;
            DFS(provider.RootTransform, 0);

            // Fallback: if no leaves were found or total is zero, set to max distance found
            if (graph.TotalLength <= 0f)
            {
                foreach (var kv in graph.DistanceFromRoot)
                {
                    if (kv.Value > graph.TotalLength)
                    {
                        graph.TotalLength = kv.Value;
                    }

                    if (kv.Value > graph.BoneLength)
                    {
                        graph.BoneLength = kv.Value;
                    }
                }
            }

            if (graph.BoneLength < 0f)
            {
                graph.BoneLength = 0f;
            }

            // Guard against zero total length
            if (graph.TotalLength <= 0f)
            {
                graph.TotalLength = 1f;
            }

            return graph;
        }

        private static List<Transform> GetValidChildren(VRCPhysBoneProviderBase provider, Transform transform)
        {
            var validChildren = new List<Transform>();
            for (int i = 0; i < transform.childCount; i++)
            {
                var child = transform.GetChild(i);
                if (provider.IgnoreTransforms == null || !provider.IgnoreTransforms.Contains(child))
                {
                    validChildren.Add(child);
                }
            }

            return validChildren;
        }

        private static List<Transform> GetChildrenToProcess(VRCPhysBoneProviderBase provider, Transform transform, int depth, List<Transform> validChildren = null)
        {
            validChildren ??= GetValidChildren(provider, transform);
            var childrenToProcess = new List<Transform>();
            bool isRoot = depth == 0;

            if (validChildren.Count > 0)
            {
                if (isRoot && validChildren.Count > 1)
                {
                    // Apply MultiChildType only at root when there are multiple children (branching)
                    switch (provider.MultiChildType)
                    {
                        case MultiChildType.Ignore:
                        case MultiChildType.First:
                            childrenToProcess.Add(validChildren[0]);
                            break;
                        case MultiChildType.Average:
                            childrenToProcess.AddRange(validChildren);
                            break;
                    }
                }
                else
                {
                    // No branching at root or not root: process all children normally
                    childrenToProcess.AddRange(validChildren);
                }
            }

            return childrenToProcess;
        }

        private static bool IsIgnoredTransform(VRCPhysBoneProviderBase provider, Transform transform)
        {
            return provider.IgnoreTransforms != null && provider.IgnoreTransforms.Contains(transform);
        }

        private static bool ShouldSkipFirstCapsule(VRCPhysBoneProviderBase provider, int depth, int validChildCount)
        {
            return depth == 0 && validChildCount > 1 && provider.MultiChildType == MultiChildType.Ignore;
        }

        private static void DrawEndpointExtension(VRCPhysBoneProviderBase provider, Transform transform, PhysBoneGraph graph)
        {
            if (graph.EndpointWorldLength <= 0f)
            {
                return;
            }

            var extensionLength = graph.EndpointWorldLength;
            if (extensionLength <= 0f)
            {
                return;
            }

            var direction = GetExtensionDirection(provider, transform);
            if (direction.sqrMagnitude <= MinimumDirectionMagnitude)
            {
                return;
            }

            var endPos = transform.position + direction * extensionLength;
            DrawPhysBoneSegmentLine(transform.position, endPos, Handles.color);

            var startNormalized = GetNormalizedDistance(graph, transform);
            var endDistance = GetDistanceFromRoot(graph, transform) + extensionLength;
            var endNormalized = graph.BoneLength > Mathf.Epsilon
                ? Mathf.Clamp01(endDistance / graph.BoneLength)
                : 1f;

            var startRadius = GetPhysBoneRadiusAtPosition(provider, transform, startNormalized);
            var endRadius = GetPhysBoneRadiusAtPosition(provider, transform, endNormalized);

            if (startRadius > Mathf.Epsilon && endRadius > Mathf.Epsilon)
            {
                DrawTaperedWireCapsule(transform.position, endPos, transform.rotation, startRadius, endRadius);
            }
        }

        private static Vector3 GetExtensionDirection(VRCPhysBoneProviderBase provider, Transform transform)
        {
            if (transform.parent != null)
            {
                var direction = (transform.position - transform.parent.position).normalized;
                if (direction.sqrMagnitude > MinimumDirectionMagnitude)
                {
                    return direction;
                }
            }

            if (provider.RootTransform != null)
            {
                var worldDirection = provider.RootTransform.TransformDirection(provider.EndpointPosition);
                if (worldDirection.sqrMagnitude > MinimumDirectionMagnitude)
                {
                    return worldDirection.normalized;
                }
            }

            if (provider.EndpointPosition.sqrMagnitude > MinimumDirectionMagnitude)
            {
                return provider.EndpointPosition.normalized;
            }

            return transform.up;
        }

        private static float GetNormalizedDistance(PhysBoneGraph graph, Transform transform)
        {
            if (graph.BoneLength <= Mathf.Epsilon)
            {
                return 0f;
            }

            return Mathf.Clamp01(GetDistanceFromRoot(graph, transform) / graph.BoneLength);
        }

        private static float GetMaxLossyScale(Transform transform)
        {
            if (transform == null)
            {
                return 1f;
            }

            return Mathf.Max(transform.lossyScale.x, transform.lossyScale.y, transform.lossyScale.z);
        }

        private static Vector3 GetPerpendicularAxis(Vector3 direction, params Vector3[] candidates)
        {
            foreach (var candidate in candidates)
            {
                var projected = Vector3.ProjectOnPlane(candidate, direction);
                if (projected.sqrMagnitude > MinimumDirectionMagnitude)
                {
                    return projected.normalized;
                }
            }

            var fallback = Vector3.Cross(direction, Vector3.up);
            if (fallback.sqrMagnitude > MinimumDirectionMagnitude)
            {
                return fallback.normalized;
            }

            fallback = Vector3.Cross(direction, Vector3.right);
            if (fallback.sqrMagnitude > MinimumDirectionMagnitude)
            {
                return fallback.normalized;
            }

            fallback = Vector3.Cross(direction, Vector3.forward);
            if (fallback.sqrMagnitude > MinimumDirectionMagnitude)
            {
                return fallback.normalized;
            }

            return Vector3.up;
        }

        private static IEnumerable<VRCPhysBone> EnumeratePhysBones(VRCPhysBoneCollider collider)
        {
            var avatarDescriptor = collider.GetComponentInParent<VRCAvatarDescriptor>();
            if (avatarDescriptor != null)
            {
                foreach (var physBone in avatarDescriptor.GetComponentsInChildren<VRCPhysBone>(true))
                {
                    yield return physBone;
                }

                yield break;
            }

            foreach (var physBone in UnityEngine.Object.FindObjectsOfType<VRCPhysBone>(true))
            {
                yield return physBone;
            }
        }

        private static bool ReferencesCollider(VRCPhysBone physBone, VRCPhysBoneCollider collider)
        {
            if (physBone == null)
            {
                return false;
            }

            var colliderList = physBone.colliders;
            if (colliderList == null)
            {
                return false;
            }

            foreach (var referencedCollider in colliderList)
            {
                if (referencedCollider == collider)
                {
                    return true;
                }
            }

            return false;
        }

        private readonly struct HandlesColorScope : IDisposable
        {
            private readonly Color previousColor;

            public HandlesColorScope(Color color)
            {
                previousColor = Handles.color;
                Handles.color = color;
            }

            public void Dispose()
            {
                Handles.color = previousColor;
            }
        }

        /// <summary>
        /// Data used for drawing normalized along actual path length.
        /// Nested types are placed at the end to satisfy analyzers' member ordering rules.
        /// </summary>
        private sealed class PhysBoneGraph
        {
            public readonly Dictionary<Transform, float> DistanceFromRoot = new Dictionary<Transform, float>();
            public float TotalLength;
            public float EndpointWorldLength;
            public float BoneLength;
        }
    }
}
