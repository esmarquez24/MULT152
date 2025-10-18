using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEditor.Overlays;
using UnityEditor.ProBuilder;
using UnityEditor.Toolbars;
using UnityEngine;
using UnityEngine.ProBuilder;
using UnityEngine.UIElements;

namespace Overdrive.ProBuilderPlus
{
    /// <summary>
    /// Todo: CortiWins: The type is dual purpose.
    /// - Central to the action UIs
    /// - tracking changes to probuilder selections.
    /// -> ProBuilderInfoOverlay has it's own tracking source in ProBuilderMeshEditor
    /// </summary>
    public static class ProBuilderPlusCore
    {
        // Events for UI updates
        public static event Action OnStatusChanged;

        // Current state
        private static ToolMode _currentEditMode = ToolMode.None;
        private static int _currentSelectionCount = 0;
        private static bool _lastHadProBuilderObjects = false;
        private static bool _lastHadNonProBuilderMeshObjects = false;
        
        // Track subscribers for automatic cleanup
        // Todo: Corti Note: Automatic cleanup does not happen.
        private static List<object> _subscribers = new List<object>();

        // CTRL key state tracking
        private static bool _ctrlKeyHeld = false;
        
        // Initialize the core system
        static ProBuilderPlusCore()
        {
            SubscribeToEvents();
        }
        
        public static void Initialize()
        {
            UpdateStatus();
        }
        
        public static void Cleanup()
        {
            UnsubscribeFromEvents();
        }
        
        public static void Subscribe(object subscriber, Action callback)
        {
            if (!_subscribers.Contains(subscriber))
            {
                _subscribers.Add(subscriber);
                OnStatusChanged += callback;
            }
        }
        
        public static void Unsubscribe(object subscriber, Action callback)
        {
            if (_subscribers.Contains(subscriber))
            {
                _subscribers.Remove(subscriber);
                OnStatusChanged -= callback;
            }
        }
        
        #region Properties
        
        public static ToolMode CurrentEditMode => _currentEditMode;

        public static int CurrentSelectionCount => _currentSelectionCount;

        public static bool HasProBuilderObjects => _lastHadProBuilderObjects;

        public static bool HasNonProBuilderMeshObjects => _lastHadNonProBuilderMeshObjects;

        public static bool CtrlKeyHeld => _ctrlKeyHeld;

        public static void SetCtrlKeyState(bool held)
        {
            _ctrlKeyHeld = held;
        }
        
        #endregion
        
        #region Action Access
        
        public static List<ActionInfo> GetEditorActions()
        {
            return ActionInfoProvider.GetEditorActions();
        }
        
        public static List<ActionInfo> GetCurrentModeActions()
        {
            return _currentEditMode switch
            {
                ToolMode.Object => ActionInfoProvider.GetObjectModeActions(),
                ToolMode.Face => ActionInfoProvider.GetFaceModeActions(),
                ToolMode.Edge => ActionInfoProvider.GetEdgeModeActions(),
                ToolMode.Vertex => ActionInfoProvider.GetVertexModeActions(),
                _ => new List<ActionInfo>(),
            };
        }

        #endregion

        #region Action Execution

        public static void ExecuteAction(ActionInfo action)
        {
            ExecuteAction(action, false);
        }

        public static void ExecuteAction(ActionInfo action, bool ctrlHeld)
        {
            if (ctrlHeld)
            {
                // Check if this action supports instant mode
                if (!action.SupportsInstantMode)
                {
                    // Fall through to normal execution
                }
                else
                {
                    // Cancel any current preview action before executing instantly
                    PreviewActionFramework.EndCurrentPreview(false);

                    // For CTRL+click on PreviewMenuActions, execute directly
                    if (action.CreatePreviewActionInstance != null)
                    {
                        ExecutePreviewActionDirectly(action);
                        return;
                    }

                    // For non-preview actions, execute normally
                    if (action.CustomAction != null)
                    {
                        action.CustomAction.Invoke();
                    }
                    else if (!string.IsNullOrEmpty(action.MenuCommand))
                    {
                        EditorApplication.ExecuteMenuItem(action.MenuCommand);
                    }
                    return;
                }
            }

            // Normal execution (no CTRL) - use preview framework
            if (action.CustomAction != null)
            {
                action.CustomAction.Invoke();
            }
            else if (!string.IsNullOrEmpty(action.MenuCommand))
            {
                EditorApplication.ExecuteMenuItem(action.MenuCommand);
            }
        }

        private static void ExecutePreviewActionDirectly(ActionInfo action)
        {
            var instance = action.CreatePreviewActionInstance();
            instance.StartPreview();
            var result = instance.ApplyChanges();
            instance.CleanupPreview();
        }
        
        public static Texture2D LoadIcon(string iconPath)
        {
            if (string.IsNullOrEmpty(iconPath))
            {
                return Resources.Load<Texture2D>("Icons/Old/ProBuilderGUI_UV_ShowTexture_On");
            }

            Texture2D icon = null;

            // If path starts with "Assets/", use AssetDatabase loading
            if (iconPath.StartsWith("Assets/"))
            {
                icon = AssetDatabase.LoadAssetAtPath<Texture2D>(iconPath);
            }
            // Otherwise, try Resources loading
            else
            {
                icon = Resources.Load<Texture2D>(iconPath);
            }

            // Fallback to default icon if loading failed
            if (icon == null)
            {
                return Resources.Load<Texture2D>("Icons/Old/ProBuilderGUI_UV_ShowTexture_On");
            }

            return icon;
        }
        
        #endregion
        
        #region UI Element Creation
        
        public static Button CreateButton(ActionInfo action)
        {
            var button = new Button();
            button.text = action.DisplayName;
            button.tooltip = action.Tooltip;

            // Use pointer events to capture modifier keys properly
            button.RegisterCallback<PointerUpEvent>(evt =>
            {
                // RightMouseButton is Context menu, so limit this to LMB
                if (evt.button == 0)
                {
                    bool ctrlHeld = evt.ctrlKey;
                    ExecuteAction(action, ctrlHeld);
                }
            });

            // Always set icon (LoadIcon provides fallback for empty/null paths)
            var iconTexture = LoadIcon(action.IconPath);
            if (iconTexture != null)
            {
                button.iconImage = iconTexture;
            }

            button.SetEnabled(action.IsEnabled());

            // Add CSS class based on instant mode support
            if (action.SupportsInstantMode)
            {
                button.AddToClassList("allow-instant");
            }
            else
            {
                button.AddToClassList("dont-allow-instant");
            }

            return button;
        }
        
        public static EditorToolbarButton CreateToolbarButton(ActionInfo action, bool iconOnly = true)
        {
            var icon = LoadIcon(action.IconPath);
            EditorToolbarButton button;
            
            if (iconOnly)
            {
                button = new EditorToolbarButton(icon, () => {
                    bool ctrlHeld = Event.current?.control == true;
                    ExecuteAction(action, ctrlHeld);
                });
                button.tooltip = action.Tooltip;
            }
            else
            {
                button = new EditorToolbarButton(action.DisplayName, () => {
                    bool ctrlHeld = Event.current?.control == true;
                    ExecuteAction(action, ctrlHeld);
                });
                if (icon != null)
                {
                    button.icon = icon;
                }
            }

            button.SetEnabled(action.IsEnabled());

            // Add CSS class based on instant mode support
            if (action.SupportsInstantMode)
            {
                button.AddToClassList("allow-instant");
            }
            else
            {
                button.AddToClassList("dont-allow-instant");
            }

            return button;
        }
        
        /// <summary>
        /// Populates the container Element with Bottons that can execute an Action.
        /// </summary>
        /// <param name="container">Container Element to be populated..</param>
        public static void PopulateEditorButtons(VisualElement container)
        {
            var editorActions = GetEditorActions();
            foreach (var action in editorActions)
            {
                var button = CreateButton(action);
                container.Add(button);
            }
        }
        
        public static void PopulateActionButtons(VisualElement container)
        {
            container.Clear();
            
            var actions = GetCurrentModeActions();
            
            if (actions.Count == 0)
            {
                var noActionsLabel = new Label("No actions available");
                container.Add(noActionsLabel);
                return;
            }
            
            foreach (var action in actions)
            {
                var button = CreateButton(action);
                container.Add(button);
            }
        }
        
        public static void PopulateToolbar(OverlayToolbar toolbar, bool iconOnly = true)
        {
            // Todo: CortiWins : Should be in overlay?
            toolbar.Clear();
            
            // Add editor buttons
            var editorActions = GetEditorActions();
            foreach (var action in editorActions)
            {
                var button = CreateToolbarButton(action, iconOnly);
                toolbar.Add(button);
            }
            
            // Add action buttons based on current mode
            UpdateStatus();
            var actions = GetCurrentModeActions();
            
            foreach (var action in actions)
            {
                var button = CreateToolbarButton(action, iconOnly);
                toolbar.Add(button);
            }
        }
        
        public static string GetActionsLabelText()
        {
            return _currentEditMode + " Actions";
        }
        
        #endregion
        
        #region Status Management
        
        public static void UpdateStatus()
        {
            // Store previous values for comparison
            var previousEditMode = _currentEditMode;
            int previousSelectionCount = _currentSelectionCount;
            
            // Update the edit mode info
            UpdateProBuilderEditModeInfo();
            
            // Check if we need to notify about changes
            bool needsUpdate = false;
            
            // Always update if element type or selection count changed
            if (_currentEditMode != previousEditMode || _currentSelectionCount != previousSelectionCount)
            {
                needsUpdate = true;
            }
            
            // For Object mode, also check if ProBuilder object selection state changed
            if (!_currentEditMode.IsEditMode())
            {
                var hasProBuilderObjects = ProBuilderFunctions.HasProBuilderObjectsInSelection();
                var hasNonProBuilderMeshObjects = ProBuilderFunctions.HasNonProBuilderMeshObjectsInSelection();
                
                if (hasProBuilderObjects != _lastHadProBuilderObjects || hasNonProBuilderMeshObjects != _lastHadNonProBuilderMeshObjects)
                {
                    needsUpdate = true;
                    _lastHadProBuilderObjects = hasProBuilderObjects;
                    _lastHadNonProBuilderMeshObjects = hasNonProBuilderMeshObjects;
                }
            }
            
            if (needsUpdate)
            {
                OnStatusChanged?.Invoke();
            }
        }
        
        private static void UpdateProBuilderEditModeInfo()
        {
            if (ToolManager.activeContextType == typeof(GameObjectToolContext))
            {
                _currentEditMode = ToolMode.Object;
                _currentSelectionCount = 0;
                return;
            }

            // Todo: CortiWins: check if active context is ACTUALLY ProBuilder
            // Two private types: 
            // -> UnityEditor.ProBuilder.PositionToolContext
            // -> UnityEditor.ProBuilder.TextureToolContext

            // Get current selection mode
            SelectMode selectMode = ProBuilderEditor.selectMode;
            _currentEditMode = selectMode.ToToolMode();
            
            // Get selection count using MeshSelection
            _currentSelectionCount = ProBuilderFunctions.GetProBuilderSelectionCount(_currentEditMode);
        }
        

        #endregion
        
        #region Event Management
        
        private static void SubscribeToEvents()
        {
            Selection.selectionChanged += UpdateStatus;
            ProBuilderEditor.selectModeChanged += OnSelectModeChanged;
            ProBuilderEditor.selectionUpdated += OnProBuilderSelectionUpdated;
            ToolManager.activeContextChanged += UpdateStatus;
        }
        
        private static void UnsubscribeFromEvents()
        {
            Selection.selectionChanged -= UpdateStatus;
            ProBuilderEditor.selectModeChanged -= OnSelectModeChanged;
            ProBuilderEditor.selectionUpdated -= OnProBuilderSelectionUpdated;
            ToolManager.activeContextChanged -= UpdateStatus;
        }
                
        private static void OnSelectModeChanged(SelectMode mode)
        {
            UpdateStatus();
        }
        
        private static void OnProBuilderSelectionUpdated(IEnumerable<ProBuilderMesh> selection)
        {
            UpdateStatus();
        }

        #endregion
    }
}
