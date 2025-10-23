using System.Runtime.CompilerServices;
using Unity.Mathematics;

namespace ExtensionMethods
{
    public static class VectorExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float3 x0z(this float2 float2)
        {
            return new float3(float2.x, 0f, float2.y);
        }
    }
}