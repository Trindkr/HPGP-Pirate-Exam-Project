using Components;
using Components.Enum;
using Systems.Fleet;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Systems
{
    [BurstCompile, UpdateAfter(typeof(FleetFlockingSystem))]
    public partial struct TradeRouteSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<JobModeSingleton>();
            state.RequireForUpdate<IslandPositionBuffer>();
        }
        
        public void OnUpdate(ref SystemState state)
        {
            var buffer = SystemAPI.GetSingletonBuffer<IslandPositionBuffer>();
            var job = new TradeRouteJob
            {
                IslandPositions = buffer
            };
            var jobModeSingleton = SystemAPI.GetSingleton<JobModeSingleton>();
            if (jobModeSingleton.JobMode == JobMode.Run)
            {
                foreach (var (transform, islandSeeker, navigation) in SystemAPI.Query<RefRO<LocalTransform>, RefRW<IslandSeeker>, RefRW<Navigation>>())
                {
                    var targetIsland = buffer[islandSeeker.ValueRW.IslandIndex].Position;
                    //Debug.DrawLine(transform.Position, targetIsland, Color.cyan);
                    var offset = targetIsland - transform.ValueRO.Position;
                    if (math.length(offset) < 70)
                    {
                        islandSeeker.ValueRW.IslandIndex = (islandSeeker.ValueRW.IslandIndex + 1) % buffer.Length;
                    }

                    navigation.ValueRW.DesiredDirection += math.normalize(offset) * 10f;
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
    public partial struct TradeRouteJob : IJobEntity
    {
        [ReadOnly] public DynamicBuffer<IslandPositionBuffer> IslandPositions;
        public void Execute(in LocalTransform transform, ref IslandSeeker islandSeeker, ref Navigation navigation)
        {
            var targetIsland = IslandPositions[islandSeeker.IslandIndex].Position;
            //Debug.DrawLine(transform.Position, targetIsland, Color.cyan);
            var offset = targetIsland - transform.Position;
            if (math.length(offset) < 70)
            {
                islandSeeker.IslandIndex = (islandSeeker.IslandIndex + 1) % IslandPositions.Length;
            }

            navigation.DesiredDirection += math.normalize(offset) * 10f;
        }
    }
}