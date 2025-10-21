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
            if (math.lengthsq(navigation.DesiredDirection) < 0.0001f) return;
            
            var acceleration = GetAcceleration(ref transform, motion, navigation);
            ApplyAcceleration(ref motion, acceleration);
            transform = transform.RotateY(motion.Speed * DeltaTime);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ApplyAcceleration(ref AngularMotion motion, float acceleration)
        {
            motion.Speed = math.clamp(motion.Speed + acceleration * DeltaTime, -motion.MaxSpeed, motion.MaxSpeed);
        }

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float GetAcceleration(ref LocalTransform transform, in AngularMotion motion, in Navigation navigation)
        {
            float2 forward = transform.Forward().xz;
            float2 target = math.normalize(navigation.DesiredDirection).xz;

            float crossY = forward.y * target.x - forward.x * target.y;
            float dot = math.clamp(math.dot(forward, target), -1f, 1f);
            float theta = math.atan2(crossY, dot);

            float desiredSpeed = math.clamp(theta, -motion.MaxSpeed, motion.MaxSpeed);
            float acceleration = math.clamp(desiredSpeed - motion.Speed, -motion.MaxAcceleration, motion.MaxAcceleration);

            return acceleration;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float AngleBetweenDegrees(float3 a, float3 b)
        {
            if (math.lengthsq(b) == 0f)
            {
                return 0f;
            }

            float radians = AngleBetweenRadians(a, b);
            float degrees = math.degrees(radians);
            Debug.Log(degrees);
            return degrees;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float AngleBetweenRadians(float3 a, float3 b)
        {
            float3 na = math.normalize(a);
            float3 nb = math.normalize(b);
            float dot = math.clamp(math.dot(na, nb), -1f, 1f);
            float angle = math.acos(dot); // unsigned angle

            float3 cross = math.cross(na, nb);
            float sign = math.sign(math.dot(cross, new float3(0f, 1f, 0f))); // positive or negative

            return angle * sign;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float MoveTowards(float current, float target, float maxDelta)
        {
            if (math.abs(target - current) <= maxDelta)
                return target;
            return current + math.sign(target - current) * maxDelta;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static quaternion MoveTowards(quaternion current, quaternion target, float maxDegreesDelta)
        {
            // Get the angle between the two rotations in degrees
            float angle = math.degrees(math.acos(math.clamp(math.abs(math.dot(current, target)), -1f, 1f)));

            // If the rotation difference is very small, just return target
            if (angle < 1e-5f)
                return target;

            // Determine interpolation fraction based on how far we can rotate this frame
            float t = math.min(1f, maxDegreesDelta / angle);

            // Spherical interpolation toward target
            return math.slerp(current, target, t);
        }
    }
}