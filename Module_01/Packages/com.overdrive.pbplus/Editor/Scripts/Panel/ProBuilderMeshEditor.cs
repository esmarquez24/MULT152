using UnityEditor;
using UnityEngine.ProBuilder;
using UnityEditor.EditorTools;

namespace Overdrive.ProBuilderPlus
{
    /// <summary>
    /// Custom Editor for the 'UnityEngine.ProBuilder.ProBuilderMesh' behaviour.<br/>
    /// Currently controls the visibility of the <see cref="ProBuilderInfoOverlay"/> through it's OnEnable/OnDisable
    /// but overrides the standard CustomEditor and exposes lots of internals.<br/>
    /// Todo: Hook the Overlay-Visibility differently.
    /// </summary>
    [CustomEditor(typeof(ProBuilderMesh))]
    public sealed class ProBuilderMeshEditor : Editor
    {
        private ProBuilderInfoOverlay m_Overlay;

        private bool m_OverlayAdded = false;

        private void OnEnable()  // Called when ProBuilder object is selected
        {
            if (m_Overlay == null)
            {
                m_Overlay = new ProBuilderInfoOverlay();
            }

            // Subscribe to context changes to show/hide overlay based on edit mode
            ToolManager.activeContextChanged += OnActiveContextChanged;

            // Check if we should show the overlay initially
            UpdateOverlayVisibility();
        }

        private void OnDisable() // Called when ProBuilder object is deselected
        {
            ToolManager.activeContextChanged -= OnActiveContextChanged;
            if (m_Overlay != null && m_OverlayAdded)
            {
                SceneView.RemoveOverlayFromActiveView(m_Overlay);
                m_OverlayAdded = false;
            }
        }

        private void OnActiveContextChanged()
        {
            UpdateOverlayVisibility();
        }

        private void UpdateOverlayVisibility()
        {
            if (m_Overlay == null) return;

            // Only show overlay when NOT in object mode (GameObjectToolContext)
            bool shouldShowOverlay = ToolManager.activeContextType != typeof(GameObjectToolContext);

            if (shouldShowOverlay && !m_OverlayAdded)
            {
                SceneView.AddOverlayToActiveView(m_Overlay);
                m_Overlay.displayed = true; // Make overlay visible by default
                m_OverlayAdded = true;
            }
            else if (!shouldShowOverlay && m_OverlayAdded)
            {
                SceneView.RemoveOverlayFromActiveView(m_Overlay);
                m_OverlayAdded = false;
            }
        }
    }
}
