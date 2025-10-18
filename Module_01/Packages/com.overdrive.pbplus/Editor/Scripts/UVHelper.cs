using System.Collections.Generic;
using UnityEngine.ProBuilder;

namespace Overdrive.ProBuilderPlus
{
    public enum UVMode
    {
        Auto,
        Manual
    }

    public static class UVModeStorage
    {
        // Todo: CortiWins; PB+ Not sure this really does something useful. O_o
        private static Dictionary<(ProBuilderMesh mesh, int faceIndex), UVMode> _faceUVModes = new Dictionary<(ProBuilderMesh, int), UVMode>();

        public static void ClearUVMode(ProBuilderMesh mesh, int faceIndex)
        {
            var key = (mesh, faceIndex);
            _faceUVModes.Remove(key);
        }

        public static UVMode GetUVMode(ProBuilderMesh mesh, int faceIndex)
        {
            var key = (mesh, faceIndex);
            return _faceUVModes.TryGetValue(key, out var mode) ? mode : UVMode.Auto;
        }

        public static void SetUVMode(ProBuilderMesh mesh, int faceIndex, UVMode mode)
        {
            var key = (mesh, faceIndex);
            _faceUVModes[key] = mode;
        }
    }
}
