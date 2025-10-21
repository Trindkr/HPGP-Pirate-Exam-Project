using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Mathematics;
using Unity.Transforms;

namespace ExtensionMethods
{
    [BurstCompile]
    public static class LocalTransformExtensions
    {
        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void MoveForward(this ref LocalTransform transform, float amountToMoveForward)
        {
            transform.Position += math.rotate(transform.Rotation, new float3(0, 0, amountToMoveForward));
        }
    }
}