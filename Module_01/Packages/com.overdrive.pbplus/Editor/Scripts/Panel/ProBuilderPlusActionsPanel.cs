using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Overdrive.ProBuilderPlus
{
    /// <summary>
    /// An dockable panel that contains ProBuilder tool functions shortcuts to menus as buttons.<br/>
    /// Functions open a PreviewOverlay that allows to change parameters or just apply with the hit of a button.<br/>
    /// </summary>
    /// <remarks>As an overlay, see <see cref="ProBuilderPlusActionsOverlay"/>.</remarks>
    public sealed class ProBuilderPlusActionsPanel : EditorWindow
    {
        private VisualElement _editorsContainer;
        private Label _actionsLabel;
        private VisualElement _actionsContainer;

        [MenuItem("Tools/ProBuilder/ProBuilder Plus Panel")]
        public static void ShowWindow()
        {
            var window = GetWindow<ProBuilderPlusActionsPanel>();
            window.titleContent = new GUIContent("ProBuilder Plus");
            window.Show();
        }

        public void CreateGUI()
        {
            // Retrieves the root visual element of this window hierarchy.
            var root = rootVisualElement;

            // Load the UXML template
            var template = Resources.Load<VisualTreeAsset>("UXML/ProBuilderPlus_Actions-Panel");
            if (template == null)
            {
                throw new System.Exception("ProBuilderPlus_Actions-Panel.uxml not found");
            }

            // Instantiate the template
            var panelRoot = template.Instantiate();
            root.Add(panelRoot);

            // Get references to named elements from UXML
            _editorsContainer = root.Q<VisualElement>("EditorButtons");
            _actionsContainer = root.Q<VisualElement>("ActionButtons");
            _actionsLabel = root.Q<Label>("ActionsLabel");

            if (_editorsContainer == null || _actionsContainer == null || _actionsLabel == null)
            {
                throw new System.Exception("Required UI elements not found in UXML template");
            }

            // Create buttons using Core methods
            ProBuilderPlusCore.PopulateEditorButtons(_editorsContainer);
            UpdateActions();
        }

        private void UpdateActions()
        {
            if (_actionsContainer == null || _actionsLabel == null) return;

            _actionsLabel.text = ProBuilderPlusCore.GetActionsLabelText();
            ProBuilderPlusCore.PopulateActionButtons(_actionsContainer);
        }

        private void OnEnable()
        {
            ProBuilderPlusCore.Initialize();
            ProBuilderPlusCore.OnStatusChanged += OnStatusChanged;
        }

        private void OnDisable()
        {
            ProBuilderPlusCore.OnStatusChanged -= OnStatusChanged;
        }

        private void OnStatusChanged()
        {
            UpdateActions();
        }
    }
}
