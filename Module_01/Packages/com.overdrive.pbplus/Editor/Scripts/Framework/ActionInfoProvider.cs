using UnityEditor;
using System.Collections.Generic;
using System;
using System.Linq;

namespace Overdrive.ProBuilderPlus
{
    /// <summary>
    /// Provides Lists of Action Info Instances for use in UI Elements.
    /// </summary>
    public static class ActionInfoProvider
    {
        public static List<ActionInfo> GetEdgeModeActions()
        {
            // Get auto-discovered actions first
            var autoActions = ActionAutoDiscovery.GetEdgeModeActions();

            // Add manual actions for backward compatibility
            var manualActions = new List<ActionInfo>(autoActions.Count + 0)
            {
                /* EXAMPLE
                new ActionInfo
                {
                    Id = "extrude",
                    DisplayName = "Extrude",
                    Tooltip = "Extrude Edges",
                    MenuCommand = "Tools/ProBuilder/Geometry/Extrude Edges",
                    IconPath = "Assets/ProBuilderPlus/Resources/Icons/Old/Edge_Extrude.png",
                    IsEnabled = () => hasEdges
                }
                */

                // Auto-registered
                /*
                Bridge - done
                Set Pivot - done
                Offset - done
                Bevel - done
                Fill - done
                Extrude - done
                Connect - done
                Subdivide - skip
                Insert Edge Loop - done
                */
            };

            return CombineActions(autoActions, manualActions);
        }

        public static List<ActionInfo> GetEditorActions()
        {
            // Get auto-discovered actions first
            var autoActions = ActionAutoDiscovery.GetEditorActions();

            // Add manual actions for backward compatibility
            var manualActions = new List<ActionInfo>(autoActions.Count + 6)
            {
                new ActionInfo
                {
                    Id = "lightmap",
                    DisplayName = "Lightmap",
                    Tooltip = "Open Lightmap UV Editor",
                    MenuCommand = "Tools/ProBuilder/Editors/Open Lightmap UV Editor",
                    IconPath = "Icons/Old/Object_LightmapUVs",
                    IsEnabled = () => true
                },
                new ActionInfo
                {
                    Id = "material",
                    DisplayName = "Material",
                    Tooltip = "Open Material Editor",
                    MenuCommand = "Tools/ProBuilder/Editors/Open Material Editor",
                    IconPath = "Icons/Old/Panel_Materials",
                    IsEnabled = () => true
                },
                new ActionInfo
                {
                    Id = "smoothing",
                    DisplayName = "Smoothing",
                    Tooltip = "Open Smoothing Editor",
                    MenuCommand = "Tools/ProBuilder/Editors/Open Smoothing Editor",
                    IconPath = "Icons/Old/Panel_Smoothing",
                    IsEnabled = () => true
                },
                new ActionInfo
                {
                    Id = "uv",
                    DisplayName = "UV",
                    Tooltip = "Open UV Editor",
                    MenuCommand = "Tools/ProBuilder/Editors/Open UV Editor",
                    IconPath = "Icons/Old/Panel_UVEditor",
                    IsEnabled = () => true
                },
                new ActionInfo
                {
                    Id = "color",
                    DisplayName = "Color",
                    Tooltip = "Open Vertex Color Editor",
                    MenuCommand = "Tools/ProBuilder/Editors/Open Vertex Color Editor",
                    IconPath = "Icons/Old/Panel_VertColors",
                    IsEnabled = () => true
                },
                new ActionInfo
                {
                    Id = "position",
                    DisplayName = "Position",
                    Tooltip = "Open Vertex Position Editor",
                    MenuCommand = "Tools/ProBuilder/Editors/Open Vertex Position Editor",
                    IconPath = "Icons/Old/Panel_Shapes",
                    IsEnabled = () => true
                }
            };

            // Combine and remove duplicates (auto-discovered takes precedence)
            return CombineActions(autoActions, manualActions);
        }

        public static List<ActionInfo> GetFaceModeActions()
        {
            // Get auto-discovered actions first
            var autoActions = ActionAutoDiscovery.GetFaceModeActions();

            // Add manual actions for backward compatibility
            var manualActions = new List<ActionInfo>(autoActions.Count + 0)
            {
                /* EXAMPLE
                new ActionInfo
                {
                    Id = "extrude",
                    DisplayName = "Extrude",
                    Tooltip = "Extrude Faces",
                    MenuCommand = "Tools/ProBuilder/Geometry/Extrude",
                    IconPath = "Assets/ProBuilderPlus/Resources/Icons/Old/Face_Extrude.png",
                    IsEnabled = () => hasFaces
                }
                */

                // Auto-registered
                /*
                Bevel - done
                Bridge - removed from face, only edge now
                ConformNormals - done
                DeleteFaces - done, as part of "Remove"
                DetachFaces - done, as "Separate"
                DuplicateFaces - done, as part of "Separate"
                Extrude - done
                FillHole - done
                FlipQuadTri - done
                FlipNormals - done
                Merge - replaced by "Remove"
                Offset - done
                SetPivot - done
                Subdivide - done
                Triangulate - done
                Inset - done
                */
            };

            return CombineActions(autoActions, manualActions);
        }

        public static List<ActionInfo> GetObjectModeActions()
        {
            // Get auto-discovered actions first
            var autoActions = ActionAutoDiscovery.GetObjectModeActions();

            // Add manual actions for backward compatibility
            var manualActions = new List<ActionInfo>(autoActions.Count + 2)
            {
                // Set as CollisionZone
                new ActionInfo
                {
                    Id = "setCollisionZone",
                    DisplayName = "Collider",
                    Tooltip = "Set the selected object(s) as a Collision Zone",
                    MenuCommand = "Tools/ProBuilder/Object/Set Collider",
                    IconPath = "Icons/Old/Entity_Trigger",
                    IsEnabled = () => ProBuilderPlusCore.HasProBuilderObjects
                },

                // Set as TriggerZone
                new ActionInfo
                {
                    Id = "setTriggerZone",
                    DisplayName = "Trigger",
                    Tooltip = "Set the selected object(s) as a Trigger Zone",
                    MenuCommand = "Tools/ProBuilder/Object/Set Trigger",
                    IconPath = "Icons/Old/Entity_Trigger",
                    IsEnabled = () => ProBuilderPlusCore.HasProBuilderObjects
                }

                // Auto-registered
                /*
                CenterPivot - done
                ConformObjectNormals - done
                FreezeTransform - done
                MergeObjects - done
                MirrorObjects - done
                TriangulateObject - done
                FlipObjectNormals - done
                */
            };

            return CombineActions(autoActions, manualActions);
        }
        public static List<ActionInfo> GetVertexModeActions()
        {
            // Get auto-discovered actions first
            var autoActions = ActionAutoDiscovery.GetVertexModeActions();

            var hasVertices = ProBuilderPlusCore.CurrentSelectionCount > 0;

            // Add manual actions for backward compatibility
            // Set
            var manualActions = new List<ActionInfo>(autoActions.Count + 0)
            {
                /* EXAMPLE
                new ActionInfo
                {
                    Id = "collapse",
                    DisplayName = "Collapse",
                    Tooltip = "Collapse Vertices",
                    MenuCommand = "Tools/ProBuilder/Geometry/Collapse Vertices",
                    IconPath = "Assets/ProBuilderPlus/Resources/Icons/Old/Vert_Collapse.png",
                    IsEnabled = () => hasVertices
                }
                */

                // Auto-registered
                /*
                Set Pivot - done
                Offset - done
                Split - skip
                Connect - done
                Weld - done
                Fill Hole - done
                Collapse - done
                */
            };

            return CombineActions(autoActions, manualActions);
        }

        /// <summary>
        /// Helper method to combine auto-discovered and manual actions, removing duplicates.
        /// Auto-discovered actions take precedence over manual ones with the same ID.
        /// </summary>
        private static List<ActionInfo> CombineActions(List<ActionInfo> autoActions, List<ActionInfo> manualActions)
        {
            var result = new List<ActionInfo>(autoActions);
            var autoIds = new HashSet<string>(autoActions.Select(a => a.Id));

            // Add manual actions that don't conflict with auto-discovered ones
            foreach (var manualAction in manualActions)
            {
                if (!autoIds.Contains(manualAction.Id))
                {
                    result.Add(manualAction);
                }
            }

            // Sort alphabetically by DisplayName
            result.Sort((a, b) => string.Compare(a.DisplayName, b.DisplayName, StringComparison.OrdinalIgnoreCase));

            return result;
        }
    }
}
