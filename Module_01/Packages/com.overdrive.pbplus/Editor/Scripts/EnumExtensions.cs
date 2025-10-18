using UnityEngine.ProBuilder;

namespace Overdrive.ProBuilderPlus
{
    /// <summary>
    /// Extensionsmethods for ProBuilder and ProBuilderPlus enums.
    /// </summary>
    public static class EnumExtensions
    {
        public static bool IsEditMode(this ToolMode toolMode)
        {
            return toolMode switch
            {
                ToolMode.Face => true,
                ToolMode.Vertex => true,
                ToolMode.Edge => true,
                _ => false,
            };
        }

        public static ToolMode ToToolMode(this SelectMode proBuilderSelectMode)
        {
            return proBuilderSelectMode switch
            {
                SelectMode.Face => ToolMode.Face,
                SelectMode.Edge => ToolMode.Edge,
                SelectMode.Vertex => ToolMode.Vertex,
                _ => ToolMode.None,
            };
        }
    }
}
