using System.Runtime.CompilerServices;
using Components;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Systems
{
    [BurstCompile]
    public partial struct TurnSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            var moveJob = new TurnJob
            {
                DeltaTime = SystemAPI.Time.DeltaTime
            };
            moveJob.ScheduleParallel(state.Dependency).Complete();
        }
    }

    [BurstCompile]
    public partial struct TurnJob : IJobEntity
    {
        public float DeltaTime;

        public void Execute(ref LocalTransform transform, ref AngularMotion motion, in Navigation navigation)
        {
            Debug.Log($"Transform Rotation: {math.Euler(transform.Rotation)}, Desired Direction: {navigation.DesiredDirection}");
            float desiredAcceleration = AngleBetweenDegrees(math.Euler(transform.Rotation), navigation.DesiredDirection);
            float clampedAcceleration = math.max(math.min(desiredAcceleration, motion.MaxSpeed), -motion.MaxSpeed);
            motion.Speed += clampedAcceleration;
            motion.Speed = math.min(motion.Speed, motion.MaxSpeed);
            transform.Rotation = math.mul(transform.Rotation, quaternion.RotateY(motion.Speed));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float AngleBetweenDegrees(float3 a, float3 b)
        {
            if (math.lengthsq(b) == 0f) return 0f;
            
            float radians = AngleBetween(a, b);
            return math.degrees(radians);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float AngleBetween(float3 a, float3 b)
        {
            float3 na = math.normalize(a);
            float3 nb = math.normalize(b);
            float dot = math.clamp(math.dot(na, nb), -1f, 1f);
            return math.acos(dot);
        }
    }
}