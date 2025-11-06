using System.Runtime.CompilerServices;
using Components;
using ExtensionMethods;
using Model;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Transforms;

namespace Systems.Helpers
{
    [BurstCompile]
    public static class Flocker
    {
        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Flock(ref Components.Fleet.Fleet fleet, in NativeArray<LocalTransform> fleetTransforms)
        {
            var center = new float2();
            var alignment = new float2();
            
            foreach (LocalTransform transform in fleetTransforms)
            {
                center += transform.Position.xz;
                alignment += transform.Forward().xz;
            }
            
            float inverseCount = 1f / fleetTransforms.Length;
            
            center *= inverseCount;
            fleet.Center = center;
            
            alignment *= inverseCount;
            fleet.Alignment = alignment;
        }
    }
}