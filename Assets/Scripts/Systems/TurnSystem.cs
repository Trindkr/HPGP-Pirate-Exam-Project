using System.Runtime.CompilerServices;
using Components;
using Components.Enum;
using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.SocialPlatforms;

namespace Systems
{
    [BurstCompile]
    [UpdateAfter(typeof(SinkingSystem)), UpdateAfter(typeof(MerchantTargetRouteSystem))]
    public partial struct TurnSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<JobModeSingleton>();
        }

        public void OnUpdate(ref SystemState state)
        {
            
            float deltaTime = SystemAPI.Time.DeltaTime;
            var job = new TurnJob
            {
                DeltaTime = deltaTime,
            };
            var jobModeSingleton = SystemAPI.GetSingleton<JobModeSingleton>();
            if (jobModeSingleton.JobMode == JobMode.Run)
            {
                foreach (var(  transform,  angularMotion, navigation ) 
                         in SystemAPI.Query<RefRW<LocalTransform>, RefRW<AngularMotion>, RefRO<Navigation>>())
                {
                    if (math.lengthsq(navigation.ValueRO.DesiredDirection) < 0.0001f) return;
            
                    var acceleration = TurnHelpers.GetAcceleration(transform.ValueRW, angularMotion.ValueRW, navigation.ValueRO);
                    angularMotion.ValueRW = TurnHelpers.ApplyAcceleration(angularMotion.ValueRW, acceleration, deltaTime);
                    transform.ValueRW = transform.ValueRO.RotateY(angularMotion.ValueRO.Speed * deltaTime);
                }
            }
            else if (jobModeSingleton.JobMode == JobMode.Schedule)
            {
                state.Dependency = job.Schedule(state.Dependency);
            }
            else
            {
                state.Dependency = job.ScheduleParallel(state.Dependency);
            }
        }
    }
    
    //[BurstCompile]
    public partial struct TurnJob : IJobEntity
    {
        public float DeltaTime;

        public void Execute(ref LocalTransform transform, ref AngularMotion angularMotion, in Navigation navigation)
        {
            if (math.lengthsq(navigation.DesiredDirection) < 0.0001f) return;
            
            var acceleration = TurnHelpers.GetAcceleration(transform, angularMotion, navigation);
            angularMotion = TurnHelpers.ApplyAcceleration(angularMotion, acceleration,  DeltaTime);
            transform = transform.RotateY(angularMotion.Speed * DeltaTime);
            // apply angular damping?
        }
    }

    public static class TurnHelpers
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static AngularMotion ApplyAcceleration(AngularMotion angularMotion, float acceleration, float deltaTime)
        {
            angularMotion.Speed = math.clamp(angularMotion.Speed + acceleration * deltaTime, -angularMotion.MaxSpeed, angularMotion.MaxSpeed);
            return angularMotion;
        }

        //[BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float GetAcceleration(in LocalTransform transform, in AngularMotion motion, in Navigation navigation)
        {
            float2 forward = transform.Forward().xz;
            float2 target = math.normalize(navigation.DesiredDirection).xz;

            float up = forward.y * target.x - forward.x * target.y; // is this float3(0f, 1f, 0f) ?
            float dot = math.clamp(math.dot(forward, target), -1f, 1f); // remove clamp ?
            float angleRadians = math.atan2(up, dot);

            float desiredSpeed = math.clamp(angleRadians, -motion.MaxSpeed, motion.MaxSpeed);
            float acceleration = math.clamp(desiredSpeed - motion.Speed, -motion.MaxAcceleration, motion.MaxAcceleration);

            return acceleration;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float AngleBetweenDegrees(float3 a, float3 b)
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
        public static float AngleBetweenRadians(float3 a, float3 b)
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