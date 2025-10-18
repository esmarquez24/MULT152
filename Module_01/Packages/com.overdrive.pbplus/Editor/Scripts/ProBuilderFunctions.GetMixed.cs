using System.Linq;
using UnityEngine;
using UnityEditor.ProBuilder;
using UnityEngine.ProBuilder;

namespace Overdrive.ProBuilderPlus
{
    // File contains static methods that get mixed values from selected faces/edges/vertices.
    // The term 'mixed' means that multiple selected items have a different value.
    public static partial class ProBuilderFunctions
    {
        public static (Material material, bool hasMixed) GetCurrentFaceMaterialWithMixed()
        {
            var selectedMeshes = MeshSelection.top.ToArray();
            if (selectedMeshes.Length == 0) return (null, false);

            Material firstMaterial = null;
            var hasMixed = false;

            foreach (var mesh in selectedMeshes)
            {
                var selectedFaces = mesh.GetSelectedFaces();
                if (selectedFaces == null) continue;

                var renderer = mesh.GetComponent<Renderer>();
                if (renderer == null || renderer.sharedMaterials == null) continue;

                foreach (var face in selectedFaces)
                {
                    var materialIndex = face.submeshIndex;
                    Material material = null;
                    if (materialIndex >= 0 && materialIndex < renderer.sharedMaterials.Length)
                    {
                        material = renderer.sharedMaterials[materialIndex];
                    }

                    if (firstMaterial == null) firstMaterial = material;
                    else if (firstMaterial != material) hasMixed = true;
                }
                if (hasMixed) break;
            }

            return (firstMaterial, hasMixed);
        }

        public static (int smoothingGroup, bool hasMixed) GetCurrentFaceSmoothingGroupWithMixed()
        {
            var selectedMeshes = MeshSelection.top.ToArray();
            if (selectedMeshes.Length == 0) return (0, false);

            int? firstSmoothingGroup = null;
            var hasMixed = false;

            foreach (var mesh in selectedMeshes)
            {
                var selectedFaces = mesh.GetSelectedFaces();
                if (selectedFaces == null) continue;

                foreach (var face in selectedFaces)
                {
                    if (firstSmoothingGroup == null) firstSmoothingGroup = face.smoothingGroup;
                    else if (firstSmoothingGroup != face.smoothingGroup) hasMixed = true;
                }
                if (hasMixed) break;
            }

            return (firstSmoothingGroup ?? 0, hasMixed);
        }

        public static (Color color, bool hasMixed) GetCurrentSelectionColorWithMixed()
        {
            var selectedMeshes = MeshSelection.top.ToArray();
            if (selectedMeshes.Length == 0) return (Color.white, false);

            Color? firstColor = null;
            var hasMixed = false;
            var currentSelectMode = ProBuilderEditor.selectMode;

            foreach (var mesh in selectedMeshes)
            {
                var vertices = mesh.GetVertices();
                if (vertices == null || vertices.Length == 0) continue;

                switch (currentSelectMode)
                {
                    case SelectMode.Face:
                        var selectedFaces = mesh.GetSelectedFaces();
                        if (selectedFaces != null)
                        {
                            foreach (var face in selectedFaces)
                            {
                                if (face.indexes.Count > 0 && face.indexes[0] < vertices.Length)
                                {
                                    var color = vertices[face.indexes[0]].color.gamma;
                                    if (firstColor == null) firstColor = color;
                                    else if (!Mathf.Approximately(Vector4.Distance(firstColor.Value, color), 0)) hasMixed = true;
                                    // Todo: CortiWins : Can we do an early break as soon as we have MIXED?
                                }
                            }
                        }
                        break;
                    case SelectMode.Edge:
                        var selectedEdges = mesh.selectedEdges;
                        if (selectedEdges != null)
                        {
                            foreach (var edge in selectedEdges)
                            {
                                if (edge.a < vertices.Length)
                                {
                                    var color = vertices[edge.a].color.gamma;
                                    if (firstColor == null) firstColor = color;
                                    else if (!Mathf.Approximately(Vector4.Distance(firstColor.Value, color), 0)) hasMixed = true;
                                }
                            }
                        }
                        break;
                    case SelectMode.Vertex:
                        var selectedVertices = mesh.selectedVertices;
                        if (selectedVertices != null)
                        {
                            foreach (var vertexIndex in selectedVertices)
                            {
                                if (vertexIndex < vertices.Length)
                                {
                                    var color = vertices[vertexIndex].color.gamma;
                                    if (firstColor == null) firstColor = color;
                                    else if (!Mathf.Approximately(Vector4.Distance(firstColor.Value, color), 0)) hasMixed = true;
                                }
                            }
                        }
                        break;
                }

                if (hasMixed)
                {
                    break;
                }
            }

            return (firstColor ?? Color.white, hasMixed);
        }

        public static (UVMode uvMode, bool hasMixed) GetCurrentUVModeWithMixed()
        {
            var selectedMeshes = MeshSelection.top.ToArray();
            if (selectedMeshes.Length == 0) return (UVMode.Auto, false);

            UVMode? firstMode = null;
            var hasMixed = false;

            foreach (var mesh in selectedMeshes)
            {
                var selectedFaces = mesh.GetSelectedFaces();
                if (selectedFaces == null) continue;

                var allFaces = mesh.faces.ToArray();

                foreach (var selectedFace in selectedFaces)
                {
                    // Find the index of this face
                    var faceIndex = -1;
                    for (var i = 0; i < allFaces.Length; i++)
                    {
                        if (allFaces[i] == selectedFace)
                        {
                            faceIndex = i;
                            break;
                        }
                    }

                    if (faceIndex != -1)
                    {
                        var faceMode = UVModeStorage.GetUVMode(mesh, faceIndex);

                        if (firstMode == null)
                        {
                            firstMode = faceMode;
                        }
                        else if (firstMode != faceMode)
                        {
                            hasMixed = true;
                            break;
                        }
                    }
                }

                if (hasMixed) break;
            }

            return (firstMode ?? UVMode.Auto, hasMixed);
        }

        internal static UVValues GetUVValuesWithMixedDetection()
        {
            var result = new UVValues();
            var selectedMeshes = MeshSelection.top.ToArray();
            if (selectedMeshes.Length == 0)
            {
                // Default values
                result.uvMode = UVMode.Auto;
                result.fill = AutoUnwrapSettings.Fill.Fit;
                result.anchor = AutoUnwrapSettings.Anchor.MiddleCenter;
                result.rotation = 0f;
                result.scale = Vector2.one;
                result.offset = Vector2.zero;
                result.group = 0;
                result.vertexColor = Color.white;
                result.material = null;
                result.smoothingGroup = 0;
                return result;
            }

            int totalFacesChecked = 0;

            AutoUnwrapSettings.Fill? firstFill = null;
            AutoUnwrapSettings.Anchor? firstAnchor = null;
            float? firstRotation = null;
            Vector2? firstScale = null;
            Vector2? firstOffset = null;
            int? firstGroup = null;

            foreach (var mesh in selectedMeshes)
            {
                var selectedFaces = mesh.GetSelectedFaces();
                if (selectedFaces == null) continue;

                foreach (var face in selectedFaces)
                {
                    var uvSettings = face.uv;

                    // Check fill
                    if (firstFill == null) firstFill = uvSettings.fill;
                    else if (firstFill != uvSettings.fill) result.hasMixedFill = true;

                    // Check anchor
                    if (firstAnchor == null) firstAnchor = uvSettings.anchor;
                    else if (firstAnchor != uvSettings.anchor) result.hasMixedAnchor = true;

                    // Check rotation
                    if (firstRotation == null) firstRotation = uvSettings.rotation;
                    else if (Mathf.Abs(firstRotation.Value - uvSettings.rotation) > 0.001f) result.hasMixedRotation = true;

                    // Check scale
                    if (firstScale == null) firstScale = uvSettings.scale;
                    else if (Vector2.Distance(firstScale.Value, uvSettings.scale) > 0.001f) result.hasMixedScale = true;

                    // Check offset
                    if (firstOffset == null) firstOffset = uvSettings.offset;
                    else if (Vector2.Distance(firstOffset.Value, uvSettings.offset) > 0.001f) result.hasMixedOffset = true;

                    // Check group (placeholder for now)
                    if (firstGroup == null) firstGroup = 0; // Default group
                    else if (firstGroup != 0) result.hasMixedGroup = true;

                    totalFacesChecked++;
                }
            }

            // Set the first values as defaults
            result.fill = firstFill ?? AutoUnwrapSettings.Fill.Fit;
            result.anchor = firstAnchor ?? AutoUnwrapSettings.Anchor.MiddleCenter;
            result.rotation = firstRotation ?? 0f;
            result.scale = firstScale ?? Vector2.one;
            result.offset = firstOffset ?? Vector2.zero;
            result.group = firstGroup ?? 0;

            return result;
        }

        /// <summary>
        /// Mixed value representation of UV values of <see cref="AutoUnwrapSettings"/>.
        /// </summary>
        internal struct UVValues
        {
            public AutoUnwrapSettings.Anchor anchor;

            public AutoUnwrapSettings.Fill fill;

            public int group;

            public bool hasMixedAnchor;

            public bool hasMixedFill;

            public bool hasMixedGroup;

            public bool hasMixedMaterial;

            public bool hasMixedOffset;

            public bool hasMixedRotation;

            public bool hasMixedScale;

            public bool hasMixedSmoothingGroup;

            // UV values
            public bool hasMixedUVMode;

            // Other values
            public bool hasMixedVertexColor;

            public Material material;

            public Vector2 offset;

            public float rotation;

            public Vector2 scale;

            public int smoothingGroup;

            // UV values
            public UVMode uvMode;

            // Other values
            public Color vertexColor;
        }
    }
}
