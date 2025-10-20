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
            if (math.lengthsq(navigation.DesiredDirection) < 0.0001f)
            {
                // motion.Speed = MoveTowards(motion.Speed, 0f, motion.MaxAcceleration * DeltaTime);
                return;
            }

            var desiredRotation = quaternion.Euler(navigation.DesiredDirection);
            transform.Rotation = MoveTowards(transform.Rotation, desiredRotation, motion.MaxSpeed * DeltaTime);

            // float angleDifference = AngleBetweenDegrees(transform.Forward(), navigation.DesiredDirection);
            // float clampedAcceleration = math.clamp(angleDifference, -motion.MaxAcceleration, motion.MaxAcceleration);
            // motion.Speed += clampedAcceleration;
            // motion.Speed = math.min(motion.Speed, motion.MaxSpeed);
            // transform.Rotation = math.mul(quaternion.RotateY(motion.Speed * DeltaTime), transform.Rotation);

            // transform.Rotation = math.mul(transform.Rotation, quaternion.RotateY(AngleBetweenRadians(transform.Forward(), navigation.DesiredDirection)));
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