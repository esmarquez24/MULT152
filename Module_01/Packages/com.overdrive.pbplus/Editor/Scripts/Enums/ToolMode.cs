namespace Overdrive.ProBuilderPlus
{
    /// <summary>
    /// Tool Modes Supported by ProBuilder Plus.
    /// </summary>
    [System.Flags]
    public enum ToolMode
    {
        /// <summary>
        /// No tool Selected.
        /// </summary>
        None = 0,

        /// <summary>
        /// Unity is in <see cref="UnityEditor.EditorTools.GameObjectToolContext"/>
        /// </summary>
        Object = 1 << 0,

        /// <summary>
        /// Unity is in ProBuilder FACE selection context.
        /// </summary>
        Face = 1 << 1,

        /// <summary>
        /// Unity is in ProBuilder EDGE selection context.
        /// </summary>
        Edge = 1 << 2,

        /// <summary>
        /// Unity is in ProBuilder VERTEX selection context.
        /// </summary>
        Vertex = 1 << 3,
    }
}
