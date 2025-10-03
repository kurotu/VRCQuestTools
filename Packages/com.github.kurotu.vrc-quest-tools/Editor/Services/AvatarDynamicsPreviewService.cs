// <copyright file="AvatarDynamicsPreviewService.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

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
        private static readonly Color PrimaryPreviewColor = Color.red;
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
            if (provider == null || provider.RootTransform == null)
            {
                return;
            }

            // Build graph info (distances and total length), then draw the PhysBone tree
            var graph = BuildPhysBoneGraph(provider);
            DrawPhysBoneTransformTree(provider, provider.RootTransform, graph, 0);

            if (includeReferencedColliders)
            {
                // Draw colliders referenced by this PhysBone
                var originalColor = Handles.color;
                Handles.color = SecondaryPreviewColor;
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
        }

        private static void DrawColliderPreview(VRCPhysBoneColliderProvider provider, bool drawRelatedPhysBones = false)
        {
            if (provider?.Component == null)
            {
                return;
            }

            var collider = provider.Component as VRCPhysBoneCollider;
            var transform = provider.Component.transform;

            // Apply world transform with collider's local position and rotation offsets
            var worldPosition = transform.TransformPoint(collider.position);
            var worldRotation = transform.rotation * collider.rotation;

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

            if (drawRelatedPhysBones && collider != null)
            {
                var originalColor = Handles.color;
                Handles.color = SecondaryPreviewColor;
                try
                {
                    foreach (var physBoneProvider in GetPhysBoneProvidersReferencingCollider(collider))
                    {
                        DrawPhysBonePreview(physBoneProvider, includeReferencedColliders: false);
                    }
                }
                finally
                {
                    Handles.color = originalColor;
                }
            }
        }

        private static IEnumerable<VRCPhysBoneProviderBase> GetPhysBoneProvidersReferencingCollider(VRCPhysBoneCollider collider)
        {
            if (collider == null)
            {
                yield break;
            }

            var visited = new HashSet<VRCPhysBone>();
            VRCPhysBone[] physBones;

            var avatarDescriptor = collider.GetComponentInParent<VRCAvatarDescriptor>();
            if (avatarDescriptor != null)
            {
                physBones = avatarDescriptor.GetComponentsInChildren<VRCPhysBone>(true);
            }
            else
            {
                physBones = Object.FindObjectsOfType<VRCPhysBone>(true);
            }

            foreach (var physBone in physBones)
            {
                if (physBone == null || !visited.Add(physBone))
                {
                    continue;
                }

                var colliderList = physBone.colliders;
                if (colliderList == null)
                {
                    continue;
                }

                foreach (var referencedCollider in colliderList)
                {
                    if (referencedCollider == collider)
                    {
                        yield return new VRCPhysBoneProvider(physBone);
                        break;
                    }
                }
            }
        }

        private static void DrawContactPreview(VRCContactBaseProvider provider)
        {
            if (provider?.Component == null)
            {
                return;
            }

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

        private static void DrawWirePlane(Vector3 center, Quaternion rotation, float size)
        {
            var halfSize = size * 0.5f;
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

            // Draw diagonals
            Handles.DrawLine(corners[0], corners[2]);
            Handles.DrawLine(corners[1], corners[3]);
        }

        private static void DrawPhysBoneTransformTree(VRCPhysBoneProviderBase provider, Transform transform, PhysBoneGraph graph, int depth)
        {
            if (transform == null || depth > 20)
            {
                return;
            }

            // Check if this transform should be included
            if (provider.IgnoreTransforms != null && provider.IgnoreTransforms.Contains(transform))
            {
                return;
            }

            // Get children to process according to provider settings
            var validChildren = GetValidChildren(provider, transform);
            var childrenToProcess = GetChildrenToProcess(provider, transform, depth, validChildren);

            // Draw capsules to children (leaf transforms don't have capsules)
            bool isRoot = depth == 0;
            bool skipFirstCapsule = isRoot && validChildren.Count > 1 && provider.MultiChildType == MultiChildType.Ignore;

            foreach (var child in childrenToProcess)
            {
                // Skip the first capsule when MultiChildType is Ignore and this is the root
                if (skipFirstCapsule)
                {
                    skipFirstCapsule = false; // Only skip the very first one
                }
                else
                {
                    DrawPhysBoneCapsule(provider, transform, child, graph);
                }

                // Recursively process children
                DrawPhysBoneTransformTree(provider, child, graph, depth + 1);
            }

            // If this is a leaf and EndpointPosition is set, draw an extra virtual segment beyond the leaf.
            if (childrenToProcess.Count == 0 && graph.EndpointWorldLength > 0f)
            {
                // Determine the world-space extension length from the root's local endpoint offset
                var extensionLength = graph.EndpointWorldLength;

                if (extensionLength > 0f)
                {
                    // Direction to extend: continue past the leaf in the direction from parent to this leaf.
                    Vector3 dir;
                    if (transform.parent != null)
                    {
                        dir = (transform.position - transform.parent.position).normalized;
                        if (dir.sqrMagnitude < 1e-6f)
                        {
                            dir = transform.up; // fallback
                        }
                    }
                    else
                    {
                        // Root without parent: use endpoint direction relative to root
                        dir = (provider.RootTransform != null)
                            ? provider.RootTransform.TransformDirection(provider.EndpointPosition.normalized)
                            : provider.EndpointPosition.normalized;
                    }

                    var endPos = transform.position + dir * extensionLength;

                    // Radii at start/end using the same transform scale context
                    var startNormalized = graph.TotalLength > 0f
                        ? Mathf.Clamp01(GetDistanceFromRoot(graph, transform) / graph.TotalLength)
                        : 0f;
                    var endNormalized = graph.TotalLength > 0f
                        ? Mathf.Clamp01((GetDistanceFromRoot(graph, transform) + extensionLength) / graph.TotalLength)
                        : 1f;
                    var startRadius = GetPhysBoneRadiusAtPosition(provider, transform, startNormalized);
                    var endRadius = GetPhysBoneRadiusAtPosition(provider, transform, endNormalized);

                    DrawTaperedWireCapsule(transform.position, endPos, startRadius, endRadius);
                }
            }
        }

        private static void DrawPhysBoneCapsule(VRCPhysBoneProviderBase provider, Transform startTransform, Transform endTransform, PhysBoneGraph graph)
        {
            var startPos = startTransform.position;
            var endPos = endTransform.position;
            var distance = Vector3.Distance(startPos, endPos);

            // Skip if transforms are too close
            if (distance <= 0.001f)
            {
                return;
            }

            // Calculate normalized positions for radius curve evaluation from actual distances
            float startNormalizedPosition = 0f;
            float endNormalizedPosition = 1f;
            if (graph.TotalLength > 0f)
            {
                startNormalizedPosition = Mathf.Clamp01(GetDistanceFromRoot(graph, startTransform) / graph.TotalLength);
                endNormalizedPosition = Mathf.Clamp01(GetDistanceFromRoot(graph, endTransform) / graph.TotalLength);
            }

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
            {
                return;
            }

            var rotation = Quaternion.FromToRotation(Vector3.up, direction);
            var up = rotation * Vector3.up;
            var right = rotation * Vector3.right;
            var forward = rotation * Vector3.forward;

            // Oval in the Up-Forward plane (axis normal = Right)
            var fromTopUF = -Vector3.Cross(right, up).normalized;
            var fromBottomUF = -Vector3.Cross(right, -up).normalized;
            Handles.DrawWireArc(endPos, right, fromTopUF, 180f, endRadius);
            Handles.DrawWireArc(startPos, right, fromBottomUF, 180f, startRadius);
            var sideATopUF = endPos + forward * endRadius;
            var sideABotUF = startPos + forward * startRadius;
            var sideBTopUF = endPos - forward * endRadius;
            var sideBBotUF = startPos - forward * startRadius;
            Handles.DrawLine(sideATopUF, sideABotUF);
            Handles.DrawLine(sideBTopUF, sideBBotUF);

            // Oval in the Up-Right plane (axis normal = Forward)
            Handles.DrawWireArc(endPos, forward, -Vector3.Cross(forward, up).normalized, 180f, endRadius);
            Handles.DrawWireArc(startPos, forward, -Vector3.Cross(forward, -up).normalized, 180f, startRadius);
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

            return radius * Mathf.Max(transform.lossyScale.x, transform.lossyScale.y, transform.lossyScale.z);
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
                }
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

        /// <summary>
        /// Data used for drawing normalized along actual path length.
        /// Nested type is placed at the end to satisfy analyzers' member ordering rules.
        /// </summary>
        private sealed class PhysBoneGraph
        {
            public readonly Dictionary<Transform, float> DistanceFromRoot = new Dictionary<Transform, float>();
            public float TotalLength;
            public float EndpointWorldLength;
        }
    }
}
