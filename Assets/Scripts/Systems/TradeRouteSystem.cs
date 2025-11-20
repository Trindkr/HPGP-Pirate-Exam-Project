using Components;
using Systems.Fleet;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Systems
{
    [BurstCompile, UpdateAfter(typeof(FleetFlockingSystem))]
    public partial struct TradeRouteSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<IslandPositionBuffer>();
        }
        
        public void OnUpdate(ref SystemState state)
        {
            var buffer = SystemAPI.GetSingletonBuffer<IslandPositionBuffer>();
            var tradeRouteJob = new TradeRouteJob
            {
                IslandPositions = buffer
            };
            state.Dependency = tradeRouteJob.ScheduleParallel(state.Dependency);
        }
    }

    [BurstCompile]
    public partial struct TradeRouteJob : IJobEntity
    {
        [ReadOnly] public DynamicBuffer<IslandPositionBuffer> IslandPositions;
        public void Execute(in LocalTransform transform, ref IslandSeeker islandSeeker, ref Navigation navigation)
        {
            var targetIsland = IslandPositions[islandSeeker.IslandIndex].Position;
            var offset = targetIsland - transform.Position;
            if (math.length(offset) < 70)
            {
                islandSeeker.IslandIndex = (islandSeeker.IslandIndex + 1) % IslandPositions.Length;
            }

            navigation.DesiredDirection += math.normalize(offset) * 1f;
        }
    }
}