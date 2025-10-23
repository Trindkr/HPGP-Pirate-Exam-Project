using System;
using Components;
using ExtensionMethods;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Systems
{
    [BurstCompile, UpdateBefore(typeof(TurnSystem))]
    public partial struct FlockingSystem : ISystem
    {
        private EntityQuery _query;

        public void OnCreate(ref SystemState state)
        {
            _query = new EntityQueryBuilder(Allocator.Persistent)
                .WithAll<LocalTransform, Navigation>()
                .Build(ref state);
        }

        public void OnUpdate(ref SystemState state)
        {
            using var transforms = _query.ToComponentDataArray<LocalTransform>(Allocator.TempJob);
            var flockingJob = new FlockingJob
            {
                Transforms = transforms,
            };
            flockingJob.ScheduleParallel(state.Dependency).Complete();
        }
    }

    [BurstCompile]
    public partial struct FlockingJob : IJobEntity
    {
        [ReadOnly] public NativeArray<LocalTransform> Transforms;
        // make configurable
        private const float MaxDistance = 500f;

        public void Execute(in LocalTransform transform, ref Navigation navigation)
        {
            float2 myPosition = transform.Position.xz;

            var nearbyCount = 1;
            var cohesion = new float2();
            var alignment = new float2();
            var separation = new float2();

            foreach (LocalTransform other in Transforms)
            {
                float2 otherPosition = other.Position.xz;
                float2 offset = otherPosition - myPosition;
                float squareDistance = math.lengthsq(offset);
                if (squareDistance is > MaxDistance or 0f or float.NaN) continue;

                nearbyCount++;
                cohesion += offset;
                alignment += other.Forward().xz;

                // gør det samme med cohesion og alignment, så skiber som er tæt på vægter højere
                separation -= offset * (1.0f / squareDistance - 1.0f / MaxDistance);
            }

            float inverseNearbyCount = 1f / nearbyCount;
            cohesion *= inverseNearbyCount;
            alignment *= inverseNearbyCount;

            // make config file for this
            const float alignmentStrength = 7f;
            const float cohesionStrength = .8f;
            const float separationStrength = 50f;

            float2 target = alignment * alignmentStrength + 
                            cohesion * cohesionStrength +
                            separation * separationStrength;

            navigation.DesiredDirection = target.x0z();
            var magnitudeSquared = math.lengthsq(navigation.DesiredDirection);
            navigation.DesiredMoveSpeed = magnitudeSquared;
        }
    }
}