using System.Linq;
using UnityEditor;
using UnityEditor.Overlays;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.ProBuilder;
using UnityEngine.ProBuilder;
using UnityEditor.EditorTools;
using System.Collections.Generic;

namespace Overdrive.ProBuilderPlus
{
    // [Overlay(typeof(SceneView), "PBi", defaultDockZone = DockZone.RightColumn)]
    public sealed class ProBuilderInfoOverlay : Overlay
    {
        private int _currentSelectionCount = 0;
        private ToolMode _currentToolMode = ToolMode.Object;
        private VisualElement _elementSelectedContainer;
        private Button _groupButton;
        // Flag to prevent recursive updates when setting values programmatically
        private bool _isUpdatingValues = false;

        // Todo: CortiWins Does this belong to ProBuilderPlusCore?
        private HashSet<int> _lastSelectedEdgeIndices = new HashSet<int>();
        private HashSet<int> _lastSelectedFaceIndices = new HashSet<int>();
        private HashSet<int> _lastSelectedVertexIndices = new HashSet<int>();

        private UnityEditor.UIElements.ObjectField _materialField;
        private bool _materialShowingMixed = false;
        private VisualElement _noElementSelectedContainer;
        private VisualElement _objectModeContainer;
        private bool _pendingUpdate = false;
        private VisualElement _root;
        private List<ProBuilderMesh> _selectedMeshes = new List<ProBuilderMesh>();
        private IntegerField _smoothingGroupField;
        private bool _smoothingGroupShowingMixed = false;
        private Button _ungroupButton;
        private int _updateValueDepth = 0;
        private EnumField _uvAnchorField;
        private bool _uvAnchorShowingMixed = false;
        private VisualElement _uvAutoItemsContainer;
        private EnumField _uvFillModeField;
        private bool _uvFillModeShowingMixed = false;
        private IntegerField _uvGroupField;
        private bool _uvGroupShowingMixed = false;
        private VisualElement _uvManualItemsContainer;
        private EnumField _uvModeField;
        private bool _uvModeShowingMixed = false;
        private Vector2Field _uvOffsetField;
        private bool _uvOffsetShowingMixed = false;
        private FloatField _uvRotationFloatField;
        private bool _uvRotationShowingMixed = false;
        private Vector2Field _uvScaleField;
        private bool _uvScaleShowingMixed = false;
        private UnityEditor.UIElements.ColorField _vertexColorField;
        // Track which fields are showing mixed values
        private bool _vertexColorShowingMixed = false;

        public ProBuilderInfoOverlay()
        {
            displayName = "PBi";
            minSize = new Vector2(80, 0);
            maxSize = new Vector2(300, float.MaxValue);
            defaultSize = new Vector2(225, 0);
        }

        public override VisualElement CreatePanelContent()
        {
            // Load UXML from Resources
            var visualTreeAsset = Resources.Load<VisualTreeAsset>("UXML/ProBuilderPlus_Inspector");
            if (visualTreeAsset == null)
            {
                var errorRoot = new VisualElement();
                errorRoot.Add(new Label("Could not load UXML file"));
                return errorRoot;
            }

            _root = visualTreeAsset.Instantiate();

            // Query for main containers
            _elementSelectedContainer = _root.Q<VisualElement>("ElementSelected");
            _noElementSelectedContainer = _root.Q<VisualElement>("NoElementSelected");
            _objectModeContainer = _root.Q<VisualElement>("ObjectMode");

            // Query for UI elements within ElementSelected container
            _vertexColorField = _elementSelectedContainer?.Q<UnityEditor.UIElements.ColorField>("VertexColor");
            _materialField = _elementSelectedContainer?.Q<UnityEditor.UIElements.ObjectField>("Material");
            _smoothingGroupField = _elementSelectedContainer?.Q<IntegerField>("SmoothingGroup");
            _uvModeField = _elementSelectedContainer?.Q<EnumField>("UV-AutoManualMode");
            _uvAutoItemsContainer = _elementSelectedContainer?.Q<VisualElement>("UV-AutoItems");
            _uvManualItemsContainer = _elementSelectedContainer?.Q<VisualElement>("UV-ManualItems");
            _uvFillModeField = _uvAutoItemsContainer?.Q<EnumField>("UV-FillMode");
            _uvAnchorField = _uvAutoItemsContainer?.Q<EnumField>("UV-Anchor");
            _uvGroupField = _uvAutoItemsContainer?.Q<IntegerField>("UV-Group");
            _uvRotationFloatField = _uvManualItemsContainer?.Q<FloatField>("UV-Rotation");
            _uvScaleField = _uvManualItemsContainer?.Q<Vector2Field>("UV-Scale");
            _uvOffsetField = _uvManualItemsContainer?.Q<Vector2Field>("UV-Offset");

            // Query for Group and Ungroup buttons
            _groupButton = _uvAutoItemsContainer?.Q<Button>("GroupSelected");
            _ungroupButton = _uvAutoItemsContainer?.Q<Button>("UngroupSelected");

            // Pan U Buttons
            {
                var buttonHost = _elementSelectedContainer.Q<VisualElement>("UV-PanButtons_U");
                var button1 = buttonHost.Q<Button>("Button1");
                var button2 = buttonHost.Q<Button>("Button2");
                var button3 = buttonHost.Q<Button>("Button3");
                var button4 = buttonHost.Q<Button>("Button4");
                var button5 = buttonHost.Q<Button>("Button5");

                button1.RegisterCallback<ClickEvent>(OnPanUVButtonsClicked);
                button2.RegisterCallback<ClickEvent>(OnPanUVButtonsClicked);
                button3.RegisterCallback<ClickEvent>(OnPanUVButtonsClicked);
                button4.RegisterCallback<ClickEvent>(OnPanUVButtonsClicked);
                button5.RegisterCallback<ClickEvent>(OnPanUVButtonsClicked);
            }

            // Pan V Buttons
            {
                var buttonHost = _elementSelectedContainer.Q<VisualElement>("UV-PanButtons_V");
                var button1 = buttonHost.Q<Button>("Button1");
                var button2 = buttonHost.Q<Button>("Button2");
                var button3 = buttonHost.Q<Button>("Button3");
                var button4 = buttonHost.Q<Button>("Button4");
                var button5 = buttonHost.Q<Button>("Button5");

                button1.RegisterCallback<ClickEvent>(OnPanUVButtonsClicked);
                button2.RegisterCallback<ClickEvent>(OnPanUVButtonsClicked);
                button3.RegisterCallback<ClickEvent>(OnPanUVButtonsClicked);
                button4.RegisterCallback<ClickEvent>(OnPanUVButtonsClicked);
                button5.RegisterCallback<ClickEvent>(OnPanUVButtonsClicked);
            }

            // Pan U Texels Buttons
            {
                var buttonHost = _elementSelectedContainer.Q<VisualElement>("UV-PanButtons_UTexels");
                var button1 = buttonHost.Q<Button>("Button1");
                var button2 = buttonHost.Q<Button>("Button2");
                var button3 = buttonHost.Q<Button>("Button3");
                var button4 = buttonHost.Q<Button>("Button4");
                var button5 = buttonHost.Q<Button>("Button5");

                button1.RegisterCallback<ClickEvent>(OnPanUVTexelButtonsClicked);
                button2.RegisterCallback<ClickEvent>(OnPanUVTexelButtonsClicked);
                button3.RegisterCallback<ClickEvent>(OnPanUVTexelButtonsClicked);
                button4.RegisterCallback<ClickEvent>(OnPanUVTexelButtonsClicked);
                button5.RegisterCallback<ClickEvent>(OnPanUVTexelButtonsClicked);
            }

            // Pan V Texels Buttons
            {
                var buttonHost = _elementSelectedContainer.Q<VisualElement>("UV-PanButtons_VTexels");
                var button1 = buttonHost.Q<Button>("Button1");
                var button2 = buttonHost.Q<Button>("Button2");
                var button3 = buttonHost.Q<Button>("Button3");
                var button4 = buttonHost.Q<Button>("Button4");
                var button5 = buttonHost.Q<Button>("Button5");

                button1.RegisterCallback<ClickEvent>(OnPanUVTexelButtonsClicked);
                button2.RegisterCallback<ClickEvent>(OnPanUVTexelButtonsClicked);
                button3.RegisterCallback<ClickEvent>(OnPanUVTexelButtonsClicked);
                button4.RegisterCallback<ClickEvent>(OnPanUVTexelButtonsClicked);
                button5.RegisterCallback<ClickEvent>(OnPanUVTexelButtonsClicked);
            }


            // Set up field properties
            if (_materialField != null)
            {
                _materialField.objectType = typeof(Material);
            }

            if (_uvModeField != null)
            {
                _uvModeField.Init(UVMode.Auto);
            }

            if (_uvFillModeField != null)
            {
                _uvFillModeField.Init(AutoUnwrapSettings.Fill.Fit);
            }

            if (_uvAnchorField != null)
            {
                _uvAnchorField.Init(AutoUnwrapSettings.Anchor.MiddleCenter);
            }

            // Set up event handlers for value changes
            SetupValueChangeHandlers();

            // Subscribe to ProBuilder events
            ProBuilderEditor.selectModeChanged += OnSelectModeChanged;
            ProBuilderEditor.selectionUpdated += OnProBuilderSelectionUpdated;
            Selection.selectionChanged += UpdateProBuilderStatus;
            ToolManager.activeContextChanged += UpdateProBuilderStatus;

            // Initialize ProBuilder status before updating display
            UpdateProBuilderStatus();

            // Update initially
            UpdateDisplay();

            return _root;
        }

        public override void OnWillBeDestroyed()
        {
            ProBuilderEditor.selectModeChanged -= OnSelectModeChanged;
            ProBuilderEditor.selectionUpdated -= OnProBuilderSelectionUpdated;
            Selection.selectionChanged -= UpdateProBuilderStatus;
            ToolManager.activeContextChanged -= UpdateProBuilderStatus;
        }

        private bool HasSelectionChanged()
        {
            if (MeshSelection.selectedObjectCount == 0) return false;
            var selectedMeshes = MeshSelection.top.ToArray();

            var currentSelectMode = ProBuilderEditor.selectMode;

            switch (currentSelectMode)
            {
                case SelectMode.Face:
                    var currentFaceIndices = ProBuilderFunctions.CreateSelectedFacesHashSet();
                    return !currentFaceIndices.SetEquals(_lastSelectedFaceIndices);

                case SelectMode.Edge:
                    var currentEdgeIndices = ProBuilderFunctions.CreateSelectedEdgesHashSet();
                    return !currentEdgeIndices.SetEquals(_lastSelectedEdgeIndices);

                case SelectMode.Vertex:
                    var currentVertexIndices = ProBuilderFunctions.CreateSelectedVerticesHashSet();
                    return !currentVertexIndices.SetEquals(_lastSelectedVertexIndices);

                default:
                    return false;
            }
        }

        private void OnProBuilderSelectionUpdated(IEnumerable<ProBuilderMesh> selection)
        {
            UpdateProBuilderStatus();
        }

        private void OnSelectModeChanged(SelectMode mode)
        {
            UpdateProBuilderStatus();
        }

        private void SetupValueChangeHandlers()
        {
            // Vertex color change handler
            if (_vertexColorField != null)
            {
                _vertexColorField.RegisterValueChangedCallback(OnVertexColorChanged);
            }

            // Material change handler (faces only)
            if (_materialField != null)
            {
                _materialField.RegisterValueChangedCallback(OnMaterialChanged);
            }

            // Smoothing group change handler (faces only)
            if (_smoothingGroupField != null)
            {
                _smoothingGroupField.RegisterValueChangedCallback(OnSmoothingGroupChanged);
            }

            // UV mode change handler (faces only)
            if (_uvModeField != null)
            {
                _uvModeField.RegisterValueChangedCallback(OnUVModeChanged);
            }

            // UV fill mode change handler (faces only)
            if (_uvFillModeField != null)
            {
                _uvFillModeField.RegisterValueChangedCallback(OnUVFillModeChanged);
            }

            // UV group change handler (faces only)
            if (_uvGroupField != null)
            {
                _uvGroupField.RegisterValueChangedCallback(OnUVGroupChanged);
            }

            // UV settings change handlers (faces only)
            if (_uvAnchorField != null)
            {
                _uvAnchorField.RegisterValueChangedCallback(OnUVAnchorChanged);
            }

            if (_uvRotationFloatField != null)
            {
                _uvRotationFloatField.RegisterValueChangedCallback(OnUVRotationChanged);
                _uvRotationFloatField.Q<TextField>()?.RegisterCallback<FocusInEvent>(evt => _uvRotationFloatField.ClearMixedStateOnFocus(ref _uvRotationShowingMixed));
            }

            if (_uvScaleField != null)
            {
                _uvScaleField.RegisterValueChangedCallback(OnUVScaleChanged);
                var xField = _uvScaleField.Q<FloatField>("unity-x-input");
                var yField = _uvScaleField.Q<FloatField>("unity-y-input");
                xField?.Q<TextField>()?.RegisterCallback<FocusInEvent>(evt => _uvScaleField.ClearMixedStateOnFocus(ref _uvScaleShowingMixed));
                yField?.Q<TextField>()?.RegisterCallback<FocusInEvent>(evt => _uvScaleField.ClearMixedStateOnFocus(ref _uvScaleShowingMixed));
            }

            if (_uvOffsetField != null)
            {
                _uvOffsetField.RegisterValueChangedCallback(OnUVOffsetChanged);
                var xField = _uvOffsetField.Q<FloatField>("unity-x-input");
                var yField = _uvOffsetField.Q<FloatField>("unity-y-input");
                xField?.Q<TextField>()?.RegisterCallback<FocusInEvent>(evt => _uvOffsetField.ClearMixedStateOnFocus(ref _uvOffsetShowingMixed));
                yField?.Q<TextField>()?.RegisterCallback<FocusInEvent>(evt => _uvOffsetField.ClearMixedStateOnFocus(ref _uvOffsetShowingMixed));
            }

            // Group and Ungroup button handlers
            if (_groupButton != null)
            {
                _groupButton.RegisterCallback<ClickEvent>(OnGroupButtonClicked);
            }

            if (_ungroupButton != null)
            {
                _ungroupButton.RegisterCallback<ClickEvent>(OnUngroupButtonClicked);
            }
        }

        private void UpdateDisplay()
        {
            if (_root == null) return;

            // Get selected ProBuilder meshes
            _selectedMeshes.Clear();

            // Todo: Gibt es von der API?
            foreach (var obj in Selection.gameObjects)
            {
                var mesh = obj.GetComponent<ProBuilderMesh>();
                if (mesh != null)
                    _selectedMeshes.Add(mesh);
            }

            // Update visibility and values
            UpdateElementVisibility();
            UpdateElementValues();
        }

        #region Value Change Handlers

        private void OnGroupButtonClicked(ClickEvent evt)
        {
            if (_currentToolMode != ToolMode.Face) return;
            ProBuilderFunctions.SetSelectedFacesToUnusedTextureGroup();
        }

        private void OnMaterialChanged(ChangeEvent<Object> evt)
        {
            if (_isUpdatingValues || _updateValueDepth > 0) return;
            if (_currentToolMode != ToolMode.Face) return;
            ProBuilderFunctions.ApplyMaterial(evt.newValue as Material);
        }

        private void OnPanUVButtonsClicked(ClickEvent evt)
        {
            if (evt.target is not Button btn)
                return;

            var shiftU = btn.parent.name == "UV-PanButtons_U" ? btn.name switch
            {
                "Button1" => 0.25f,
                "Button2" => 0.125f,
                "Button3" => 0.0625f,
                "Button4" => 0.015625f,
                "Button5" => 0.0078125f,
                _ => 0.0f,
            } : 0.0f;

            var shiftV = btn.parent.name == "UV-PanButtons_V" ? btn.name switch
            {
                "Button1" => 0.25f,
                "Button2" => 0.125f,
                "Button3" => 0.0625f,
                "Button4" => 0.015625f,
                "Button5" => 0.0078125f,
                _ => 0.0f,
            } : 0.0f;

            ProBuilderFunctions.AppplyUVOffsetPan(new Vector2(
                evt.shiftKey ? -shiftU : shiftU,
                evt.shiftKey ? -shiftV : shiftV));
        }

        private void OnPanUVTexelButtonsClicked(ClickEvent evt)
        {
            if (evt.target is not Button btn)
                return;

            var shiftU = btn.parent.name == "UV-PanButtons_UTexels" ? btn.name switch
            {
                "Button1" => 256,
                "Button2" => 128,
                "Button3" => 32,
                "Button4" => 8,
                "Button5" => 1,
                _ => 0,
            } : 0;

            var shiftV = btn.parent.name == "UV-PanButtons_VTexels" ? btn.name switch
            {
                "Button1" => 256,
                "Button2" => 128,
                "Button3" => 32,
                "Button4" => 8,
                "Button5" => 1,
                _ => 0,
            } : 0;

            ProBuilderFunctions.ApplyUVOffsetPanTexels(new Vector2(
                (evt.shiftKey ? -shiftU : shiftU),
                (evt.shiftKey ? -shiftV : shiftV)));
        }

        private void OnSmoothingGroupChanged(ChangeEvent<int> evt)
        {
            if (_isUpdatingValues || _updateValueDepth > 0) return;
            if (_currentToolMode != ToolMode.Face) return;
            ProBuilderFunctions.ApplySmoothingGroup(evt.newValue);
        }

        private void OnUngroupButtonClicked(ClickEvent evt)
        {
            if (_currentToolMode != ToolMode.Face) return;
            ProBuilderFunctions.UngroupSelectedFaces();
        }

        private void OnUVAnchorChanged(ChangeEvent<System.Enum> evt)
        {
            if (_isUpdatingValues || _updateValueDepth > 0) return;
            if (_currentToolMode != ToolMode.Face) return;
            ProBuilderFunctions.ApplyUVAnchor((AutoUnwrapSettings.Anchor)evt.newValue);

        }

        private void OnUVFillModeChanged(ChangeEvent<System.Enum> evt)
        {
            if (_isUpdatingValues || _updateValueDepth > 0) return;
            if (_currentToolMode != ToolMode.Face) return;

            ProBuilderFunctions.ApplyUVFillMode((AutoUnwrapSettings.Fill)evt.newValue);

        }

        private void OnUVGroupChanged(ChangeEvent<int> evt)
        {
            if (_isUpdatingValues || _updateValueDepth > 0) return;
            if (_currentToolMode != ToolMode.Face) return;

            ProBuilderFunctions.ApplyUVGroup(evt.newValue);

        }

        private void OnUVModeChanged(ChangeEvent<System.Enum> evt)
        {
            if (_isUpdatingValues || _updateValueDepth > 0) return;
            if (_currentToolMode != ToolMode.Face) return;

            var uvMode = (UVMode)evt.newValue;
            UpdateUVModeVisibility(uvMode);
            ProBuilderFunctions.ApplyUVMode(uvMode);

            // Update visibility based on the new mode
            UpdateUVModeVisibility(uvMode);

        }

        private void OnUVOffsetChanged(ChangeEvent<Vector2> evt)
        {
            if (_isUpdatingValues || _updateValueDepth > 0) return;
            if (_currentToolMode == ToolMode.Face)
            {
                ProBuilderFunctions.ApplyUVOffset(evt.newValue);
            }
        }

        private void OnUVRotationChanged(ChangeEvent<float> evt)
        {
            if (_isUpdatingValues || _updateValueDepth > 0) return;
            if (_currentToolMode == ToolMode.Face)
            {
                ProBuilderFunctions.ApplyUVRotation(evt.newValue);
            }
        }

        private void OnUVScaleChanged(ChangeEvent<Vector2> evt)
        {
            if (_isUpdatingValues || _updateValueDepth > 0) return;
            if (_currentToolMode == ToolMode.Face)
            {
                ProBuilderFunctions.ApplyUVScale(evt.newValue);
            }
        }

        private void OnVertexColorChanged(ChangeEvent<Color> evt)
        {
            if (_isUpdatingValues || _updateValueDepth > 0) return;
            ProBuilderFunctions.ApplyVertexColor(evt.newValue);
        }
        #endregion

        private void UpdateElementValues()
        {
            // Only update values when ElementSelected container is visible
            if (_elementSelectedContainer == null || _elementSelectedContainer.style.display != DisplayStyle.Flex)
                return;

            // Set flags to prevent recursive updates during programmatic value changes
            _updateValueDepth++;
            _isUpdatingValues = true;

            try
            {
                // Show/hide UI elements based on mode
                bool isVertexOrEdgeMode = _currentToolMode == ToolMode.Vertex || _currentToolMode == ToolMode.Edge;
                bool isFaceMode = _currentToolMode == ToolMode.Face;

                // Vertex color - always visible
                if (_vertexColorField != null)
                {
                    _vertexColorField.style.display = DisplayStyle.Flex;
                    var (vertexColor, hasMixedVertexColor) = ProBuilderFunctions.GetCurrentSelectionColorWithMixed();
                    _vertexColorField.SetColorFieldMixed(hasMixedVertexColor, vertexColor, ref _vertexColorShowingMixed);
                }

                // Material - only visible in face mode
                if (_materialField != null)
                {
                    _materialField.style.display = isFaceMode ? DisplayStyle.Flex : DisplayStyle.None;
                    if (isFaceMode)
                    {
                        var (material, hasMixedMaterial) = ProBuilderFunctions.GetCurrentFaceMaterialWithMixed();
                        _materialField.SetObjectFieldMixed(hasMixedMaterial, material, ref _materialShowingMixed);
                    }
                }

                // Smoothing group - only visible in face mode
                if (_smoothingGroupField != null)
                {
                    _smoothingGroupField.style.display = isFaceMode ? DisplayStyle.Flex : DisplayStyle.None;
                    if (isFaceMode)
                    {
                        var (smoothingGroup, hasMixedSmoothingGroup) = ProBuilderFunctions.GetCurrentFaceSmoothingGroupWithMixed();
                        _smoothingGroupField.SetIntegerFieldMixed(hasMixedSmoothingGroup, smoothingGroup, ref _smoothingGroupShowingMixed);
                    }
                }

                // UV mode - only visible in face mode
                if (_uvModeField != null)
                {
                    _uvModeField.style.display = isFaceMode ? DisplayStyle.Flex : DisplayStyle.None;
                    if (isFaceMode)
                    {
                        var (uvMode, hasMixedUVMode) = ProBuilderFunctions.GetCurrentUVModeWithMixed();
                        _uvModeField.SetEnumFieldMixed(hasMixedUVMode, uvMode, ref _uvModeShowingMixed);
                        UpdateUVModeVisibility(hasMixedUVMode ? UVMode.Auto : uvMode); // Use Auto for visibility when mixed
                    }
                }

                // UV containers - only visible in face mode
                if (_uvAutoItemsContainer != null)
                {
                    _uvAutoItemsContainer.style.display = isFaceMode ? DisplayStyle.Flex : DisplayStyle.None;
                }

                if (_uvManualItemsContainer != null)
                {
                    _uvManualItemsContainer.style.display = isFaceMode ? DisplayStyle.Flex : DisplayStyle.None;
                }

                // Update UV settings (only for face selections)
                if (isFaceMode)
                {
                    var uvValues = ProBuilderFunctions.GetUVValuesWithMixedDetection();

                    _uvFillModeField.SetEnumFieldMixed(uvValues.hasMixedFill, uvValues.fill, ref _uvFillModeShowingMixed);
                    _uvGroupField.SetIntegerFieldMixed(uvValues.hasMixedGroup, uvValues.group, ref _uvGroupShowingMixed);
                    _uvAnchorField.SetEnumFieldMixed(uvValues.hasMixedAnchor, uvValues.anchor, ref _uvAnchorShowingMixed);
                    _uvRotationFloatField.SetFloatFieldMixed(uvValues.hasMixedRotation, uvValues.rotation, ref _uvRotationShowingMixed);
                    _uvScaleField.SetVector2FieldMixed(uvValues.hasMixedScale, uvValues.scale, ref _uvScaleShowingMixed);
                }
            }
            finally
            {
                // Always reset flags after updating values
                _isUpdatingValues = false;
                _updateValueDepth--;
            }
        }

        private void UpdateElementVisibility()
        {
            bool hasProBuilderSelection = _selectedMeshes.Count > 0;
            bool isInEditMode = ToolManager.activeContextType != typeof(GameObjectToolContext);
            bool hasElementSelection = (_currentToolMode == ToolMode.Vertex || _currentToolMode == ToolMode.Edge || _currentToolMode == ToolMode.Face) && _currentSelectionCount > 0;

            // Hide all containers first
            if (_elementSelectedContainer != null)
                _elementSelectedContainer.style.display = DisplayStyle.None;
            if (_noElementSelectedContainer != null)
                _noElementSelectedContainer.style.display = DisplayStyle.None;
            if (_objectModeContainer != null)
                _objectModeContainer.style.display = DisplayStyle.None;

            // Show appropriate container based on mode and selection
            if (!isInEditMode)
            {
                // Object mode - show ObjectMode container
                if (_objectModeContainer != null)
                    _objectModeContainer.style.display = DisplayStyle.Flex;
            }
            else if (hasElementSelection)
            {
                // Element mode with selection - show ElementSelected container
                if (_elementSelectedContainer != null)
                {
                    _elementSelectedContainer.style.display = DisplayStyle.Flex;
                }
            }
            else
            {
                // Element mode without selection - show NoElementSelected container
                if (_noElementSelectedContainer != null)
                    _noElementSelectedContainer.style.display = DisplayStyle.Flex;
            }
        }

        private void UpdateProBuilderStatus()
        {
            // Check if we need to update
            bool needsUpdate = false;

            var toolMode = ProBuilderEditor.selectMode.ToToolMode();
            int selectionCount = ProBuilderFunctions.GetProBuilderSelectionCount(toolMode);

            // Always update if element type or selection count changed
            if (_currentToolMode != toolMode || _currentSelectionCount != selectionCount)
            {
                needsUpdate = true;
            }

            // Check if the actual selected elements changed (even if count is the same)
            if (!needsUpdate)
            {
                needsUpdate = HasSelectionChanged();
            }

            if (needsUpdate)
            {
                _currentToolMode = toolMode;
                _currentSelectionCount = selectionCount;
                UpdateSelectedElementsCache();

                // Use delayed update to prevent rapid-fire updates during drag selection
                if (!_pendingUpdate)
                {
                    _pendingUpdate = true;
                    EditorApplication.delayCall += () =>
                    {
                        _pendingUpdate = false;
                        if (this != null) // Check if overlay still exists
                        {
                            UpdateDisplay();
                        }
                    };
                }
            }
        }

        private void UpdateSelectedElementsCache()
        {
            var currentSelectMode = ProBuilderEditor.selectMode;
            switch (currentSelectMode)
            {
                case SelectMode.Face:
                    _lastSelectedFaceIndices = ProBuilderFunctions.CreateSelectedFacesHashSet();
                    break;

                case SelectMode.Edge:
                    _lastSelectedEdgeIndices = ProBuilderFunctions.CreateSelectedEdgesHashSet();
                    break;

                case SelectMode.Vertex:
                    _lastSelectedVertexIndices = ProBuilderFunctions.CreateSelectedVerticesHashSet();
                    break;
            }
        }

        private void UpdateUVModeVisibility(UVMode uvMode)
        {
            if (_uvAutoItemsContainer != null)
            {
                // Auto items are enabled only in Auto mode, disabled in Manual or Mixed
                _uvAutoItemsContainer.SetEnabled(uvMode == UVMode.Auto);
            }

            if (_uvManualItemsContainer != null)
            {
                // Manual items are always visible
                _uvManualItemsContainer.style.display = DisplayStyle.Flex;
            }
        }
    }
}
