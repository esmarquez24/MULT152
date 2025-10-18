using System.Linq;
using UnityEditor;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.ProBuilder;
using UnityEngine.ProBuilder;

namespace Overdrive.ProBuilderPlus
{
    // File contains static methods that change the UV settings of selected ProBuilder faces.
    public static partial class ProBuilderFunctions
    {
        public static void ApplyUVAnchor(AutoUnwrapSettings.Anchor anchor)
        {
            var selectedMeshes = MeshSelection.top.ToArray();
            if (selectedMeshes.Length == 0) return;

            Undo.RecordObjects(selectedMeshes, "Change UV Anchor");

            foreach (var mesh in selectedMeshes)
            {
                var selectedFaces = mesh.GetSelectedFaces();
                if (selectedFaces == null) continue;

                IList<Face> allFaces = mesh.faces;
                if (allFaces == null) continue;

                foreach (var face in selectedFaces)
                {
                    var uvSettings = face.uv;
                    uvSettings.anchor = anchor;
                    face.uv = uvSettings;
                    SetUVSettingsToGroup(uvSettings, face.textureGroup, allFaces);
                }

                mesh.ToMesh();
                mesh.Refresh();
            }
        }

        public static void ApplyUVFillMode(AutoUnwrapSettings.Fill fillMode)
        {
            var selectedMeshes = MeshSelection.top.ToArray();
            if (selectedMeshes.Length == 0) return;

            Undo.RecordObjects(selectedMeshes, "Change UV Fill Mode");

            foreach (var mesh in selectedMeshes)
            {
                var selectedFaces = mesh.GetSelectedFaces();
                if (selectedFaces == null) continue;

                IList<Face> allFaces = mesh.faces;
                if (allFaces == null) continue;

                foreach (var face in selectedFaces)
                {
                    var uvSettings = face.uv;
                    uvSettings.fill = fillMode;
                    face.uv = uvSettings;
                    SetUVSettingsToGroup(uvSettings, face.textureGroup, allFaces);
                }

                mesh.ToMesh();
                mesh.Refresh();
            }
        }

        public static void ApplyUVGroup(int uvGroup)
        {
            var selectedMeshes = MeshSelection.top.ToArray();
            if (selectedMeshes.Length == 0) return;

            Undo.RecordObjects(selectedMeshes, "Change UV Group");

            foreach (var mesh in selectedMeshes)
            {
                var selectedFaces = mesh.GetSelectedFaces();
                if (selectedFaces == null) continue;

                foreach (var face in selectedFaces)
                {
                    // Note: UV Group might need to be stored as a custom property
                    // since ProBuilder's AutoUnwrapSettings doesn't have a direct equivalent
                    // This would need to be implemented based on how UV groups are used
                }

                mesh.ToMesh();
                mesh.Refresh();
            }
        }

        public static void ApplyUVMode(UVMode uvMode)
        {
            var selectedMeshes = MeshSelection.top.ToArray();
            if (selectedMeshes.Length == 0) return;

            Undo.RecordObjects(selectedMeshes, "Change UV Mode");

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
                        UVModeStorage.SetUVMode(mesh, faceIndex, uvMode);
                    }
                }
            }
        }
        public static void ApplyUVOffset(Vector2 offset)
        {
            var selectedMeshes = MeshSelection.top.ToArray();
            if (selectedMeshes.Length == 0) return;

            Undo.RecordObjects(selectedMeshes, "Change UV Offset");

            foreach (var mesh in selectedMeshes)
            {
                var selectedFaces = mesh.GetSelectedFaces();
                if (selectedFaces == null) continue;

                IList<Face> allFaces = mesh.faces;
                if (allFaces == null) continue;

                foreach (var face in selectedFaces)
                {
                    var uvSettings = face.uv;
                    uvSettings.offset = offset;
                    face.uv = uvSettings;
                    SetUVSettingsToGroup(uvSettings, face.textureGroup, allFaces);
                }

                mesh.ToMesh();
                mesh.Refresh();
            }
        }

        public static void ApplyUVOffsetPanTexels(Vector2 offsetTexels)
        {
            var selectedMeshes = MeshSelection.top.ToArray();
            if (selectedMeshes.Length == 0) return;

            Undo.RecordObjects(selectedMeshes, "Change UV Offset");

            foreach (var mesh in selectedMeshes)
            {
                var selectedFaces = mesh.GetSelectedFaces();
                if (selectedFaces == null) continue;

                IList<Face> allFaces = mesh.faces;
                if (allFaces == null) continue;

                if (!mesh.TryGetComponent<Renderer>(out var renderer)) continue;

                if (renderer.sharedMaterials == null) continue;

                foreach (var face in selectedFaces)
                {
                    int materialIndex = face.submeshIndex;
                    if (materialIndex < 0 || materialIndex >= renderer.sharedMaterials.Length)
                    {
                        continue;
                    }

                    var material = renderer.sharedMaterials[materialIndex];
                    if (material.mainTexture == null)
                    {
                        continue;
                    }

                    // From Texel Size to relative UV Values.
                    var w = (float)material.mainTexture.width;
                    var h = (float)material.mainTexture.width;
                    var uvOffset = new Vector2(
                        offsetTexels.x / w,
                        offsetTexels.y / h);

                    var uvSettings = face.uv;
                    uvSettings.offset += uvOffset;
                    face.uv = uvSettings;
                    SetUVSettingsToGroup(uvSettings, face.textureGroup, allFaces);
                }

                mesh.ToMesh();
                mesh.Refresh();
            }
        }

        public static void ApplyUVRotation(float rotation)
        {
            var selectedMeshes = MeshSelection.top.ToArray();
            if (selectedMeshes.Length == 0) return;

            Undo.RecordObjects(selectedMeshes, "Change UV Rotation");

            int totalFacesUpdated = 0;
            foreach (var mesh in selectedMeshes)
            {
                var selectedFaces = mesh.GetSelectedFaces();
                if (selectedFaces == null) continue;

                IList<Face> allFaces = mesh.faces;
                if (allFaces == null) continue;

                foreach (var face in selectedFaces)
                {
                    var uvSettings = face.uv;
                    uvSettings.rotation = rotation;
                    face.uv = uvSettings;
                    SetUVSettingsToGroup(uvSettings, face.textureGroup, allFaces);
                    totalFacesUpdated++;
                }

                mesh.ToMesh();
                mesh.Refresh();
            }
        }

        public static void ApplyUVScale(Vector2 scale)
        {
            var selectedMeshes = MeshSelection.top.ToArray();
            if (selectedMeshes.Length == 0) return;

            Undo.RecordObjects(selectedMeshes, "Change UV Scale");

            foreach (var mesh in selectedMeshes)
            {
                var selectedFaces = mesh.GetSelectedFaces();
                if (selectedFaces == null) continue;

                IList<Face> allFaces = mesh.faces;
                if (allFaces == null) continue;

                foreach (var face in selectedFaces)
                {
                    var uvSettings = face.uv;
                    uvSettings.scale = scale;
                    face.uv = uvSettings;
                    SetUVSettingsToGroup(uvSettings, face.textureGroup, allFaces);
                }

                mesh.ToMesh();
                mesh.Refresh();
            }
        }

        public static void AppplyUVOffsetPan(Vector2 offset)
        {
            var selectedMeshes = MeshSelection.top.ToArray();
            if (selectedMeshes.Length == 0) return;

            Undo.RecordObjects(selectedMeshes, "Change UV Offset");

            foreach (var mesh in selectedMeshes)
            {
                var selectedFaces = mesh.GetSelectedFaces();
                if (selectedFaces == null) continue;

                IList<Face> allFaces = mesh.faces;
                if (allFaces == null) continue;

                foreach (var face in selectedFaces)
                {
                    var uvSettings = face.uv;
                    uvSettings.offset += offset;
                    face.uv = uvSettings;
                    SetUVSettingsToGroup(uvSettings, face.textureGroup, allFaces);
                }

                mesh.ToMesh();
                mesh.Refresh();
            }
        }
        public static AutoUnwrapSettings.Fill GetCurrentUVFillMode()
        {
            var selectedMeshes = MeshSelection.top.ToArray();
            if (selectedMeshes.Length == 0) return AutoUnwrapSettings.Fill.Fit;

            var mesh = selectedMeshes[0];
            var selectedFaces = mesh.GetSelectedFaces();
            if (selectedFaces == null || selectedFaces.Length == 0) return AutoUnwrapSettings.Fill.Fit;

            // Return the fill mode from the first selected face
            return selectedFaces[0].uv.fill;
        }

        public static int GetCurrentUVGroup()
        {
            // Default to group 0 for now
            // This could be stored as a preference or per-face custom property
            return 0;
        }

        public static UVMode GetCurrentUVMode()
        {
            var (uvMode, _) = ProBuilderFunctions.GetCurrentUVModeWithMixed();
            return uvMode;
        }

        public static AutoUnwrapSettings? GetCurrentUVSettings()
        {
            var selectedMeshes = MeshSelection.top.ToArray();
            if (selectedMeshes.Length == 0) return null;

            var mesh = selectedMeshes[0];
            var selectedFaces = mesh.GetSelectedFaces();
            if (selectedFaces == null || selectedFaces.Length == 0) return null;

            // Return the UV settings from the first selected face
            return selectedFaces[0].uv;
        }

        /// <summary>
        /// Applies the given uvSettings to all faces that match the given textureGroup if it is valid ( bigger than 0 ).
        /// </summary>
        /// <param name="uvSettings">Settings to write.</param>
        /// <param name="textureGroup">ID of the textureGroup. Values above 0 indicate a textureGroup.</param>
        /// <param name="allFaces">Collection of all faces in the mesh.</param>
        public static void SetUVSettingsToGroup(AutoUnwrapSettings uvSettings, int textureGroup, IList<Face> allFaces)
        {
            // Acts like 'UnityEngine.ProBuilder.ProBuilderMesh.SetGroupUV'
            if (textureGroup <= 0)
                return;

            foreach (var face in allFaces)
            {
                if (face.textureGroup != textureGroup)
                    continue;

                face.uv = uvSettings;
            }
        }
    }
}
