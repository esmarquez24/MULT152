using System;
using UnityEditor.ProBuilder;
using UnityEngine;

namespace Overdrive.ProBuilderPlus
{
    /// <summary>
    /// Todo: Corti Notes: Not sure if this is a public use class by design.
    /// ProBuilderCore says yes
    /// UIs and Creation pattern say No
    /// </summary>
    public sealed class ActionInfo
    {
        public Type ActionType { get; private set; }

        public ProBuilderPlusActionAttribute CachedAttribute { get; private set; }

        public Func<PreviewMenuAction> CreatePreviewActionInstance { get; private set; }

        public Action CustomAction { get; private set; }
        public string DisplayName { get; set; }

        public string IconPath { get; set; }

        public string Id { get; set; }

        public Func<bool> IsEnabled { get; set; }

        public string MenuCommand { get; set; }

        public bool SupportsInstantMode { get; private set; } = true;

        public string Tooltip { get; set; }

        /// <summary>
        /// Create ActionInfo from a type and its attribute.
        /// </summary>
        internal static ActionInfo CreateActionInfoFromAttribute(Type actionType, ProBuilderPlusActionAttribute attribute)
        {
            try
            {
                var actionInfo = new ActionInfo
                {
                    Id = attribute.Id,
                    DisplayName = attribute.DisplayName,
                    Tooltip = attribute.Tooltip ?? attribute.DisplayName,
                    IconPath = attribute.IconPath,
                    MenuCommand = attribute.MenuCommand,
                    SupportsInstantMode = attribute.SupportsInstantMode,
                    ActionType = actionType,
                    CachedAttribute = attribute,
                };

                // Set up enabled check and custom action
                if (typeof(IProBuilderPlusAction).IsAssignableFrom(actionType))
                {
                    // Cache a single instance for IProBuilderPlusAction implementations
                    IProBuilderPlusAction cachedInstance = null;

                    actionInfo.IsEnabled = () =>
                    {
                        try
                        {
                            if (cachedInstance == null)
                                cachedInstance = Activator.CreateInstance(actionType) as IProBuilderPlusAction;
                            return cachedInstance?.IsActionEnabled ?? false;
                        }
                        catch
                        {
                            return false;
                        }
                    };

                    actionInfo.CustomAction = () =>
                    {
                        try
                        {
                            if (cachedInstance == null)
                                cachedInstance = Activator.CreateInstance(actionType) as IProBuilderPlusAction;
                            cachedInstance?.ExecuteAction();
                        }
                        catch (Exception e)
                        {
                            Debug.LogError($"Error executing action {attribute.DisplayName}: {e.Message}");
                        }
                    };
                }
                else if (typeof(MenuAction).IsAssignableFrom(actionType))
                {
                    // Cache a single instance for MenuAction implementations
                    MenuAction cachedInstance = null;

                    actionInfo.IsEnabled = () =>
                    {
                        try
                        {
                            if (cachedInstance == null)
                                cachedInstance = Activator.CreateInstance(actionType) as MenuAction;
                            return cachedInstance?.enabled ?? false;
                        }
                        catch
                        {
                            return false;
                        }
                    };

                    actionInfo.CustomAction = () =>
                    {
                        try
                        {
                            if (cachedInstance == null)
                                cachedInstance = Activator.CreateInstance(actionType) as MenuAction;
                            cachedInstance?.PerformAction();
                        }
                        catch (Exception e)
                        {
                            Debug.LogError($"Error executing action {attribute.DisplayName}: {e.Message}");
                        }
                    };

                    // If it's a PreviewMenuAction, add the factory method
                    if (typeof(PreviewMenuAction).IsAssignableFrom(actionType))
                    {
                        actionInfo.CreatePreviewActionInstance = () =>
                        {
                            try
                            {
                                var previewMenuAction = Activator.CreateInstance(actionType) as PreviewMenuAction;
                                previewMenuAction?.SetCachedAttribute(attribute);
                                return previewMenuAction;
                            }
                            catch (Exception e)
                            {
                                Debug.LogError($"Error creating PreviewMenuAction instance for {attribute.DisplayName}: {e.Message}");
                                return null;
                            }
                        };
                    }
                }
                else
                {
                    // For other types, assume they're always enabled
                    actionInfo.IsEnabled = () => true;
                    actionInfo.CustomAction = () =>
                    {
                        Debug.LogWarning($"Action {attribute.DisplayName} does not implement IProBuilderPlusAction or MenuAction");
                    };
                }

                return actionInfo;
            }
            catch (Exception e)
            {
                Debug.LogError($"Error creating ActionInfo for {actionType.Name}: {e.Message}");
                return null;
            }
        }
    }
}
