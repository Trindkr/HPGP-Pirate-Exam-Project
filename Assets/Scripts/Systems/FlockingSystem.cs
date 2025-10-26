using System;
using Components;
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
            
        }
    }
}