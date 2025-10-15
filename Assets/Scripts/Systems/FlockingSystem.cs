using Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Systems
{
    [BurstCompile]
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
        private const float MaxDistance = 500f;

        public void Execute(in LocalTransform transform, ref Navigation navigation)
        {
            float2 myPosition = transform.Position.xz;

            var nearbyCount = 1;
            var center = new float2();
            var alignment = new float2();
            var separation = new float2();

            foreach (LocalTransform other in Transforms)
            {
                var otherPosition = other.Position.xz;
                var offset = otherPosition - myPosition;
                var squareDistance = math.lengthsq(offset);
                if (squareDistance is > MaxDistance or 0) continue;

                nearbyCount++;
                center += offset;
                alignment += other.Forward().xz;

                separation -= offset * (1.0f / squareDistance - 1.0f / MaxDistance);
            }

            var average = 1f / nearbyCount;
            center *= average;
            alignment *= average;

            float2 target = center + alignment + separation;

            navigation.DesiredDirection = new float3(target.x, 0, target.y);
            navigation.DesiredMoveSpeed = 5f;
        }
    }
}