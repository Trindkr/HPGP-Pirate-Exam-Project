using Components;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Systems
{
    [BurstCompile, UpdateAfter(typeof(FlockingSystem))]
    public partial struct TradeRouteSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            var tradeRouteJob = new TradeRouteJob
            {
            };
            state.Dependency = tradeRouteJob.ScheduleParallel(state.Dependency);
        }
    }

    [BurstCompile]
    public partial struct TradeRouteJob : IJobEntity
    {
        private static readonly float3[] Islands =
        {
            new(-132.2f, 0f, 243.5f),
            new(-368f, 0f, 24f),
            new(-173.4f, 0, -188.6f),
            new(143.2f, -2, -90.2f),
            new(163f, 0f, 222f)
        };

        public void Execute(ref LocalTransform transform, ref Merchant merchant, ref Navigation navigation)
        {
            var targetIsland = Islands[merchant.IslandIndex];
            var offset = targetIsland - transform.Position;
            if (math.lengthsq(offset) < 100)
            {
                merchant.IslandIndex = (merchant.IslandIndex + 1) % Islands.Length;
            }

            navigation.DesiredDirection += math.normalize(offset) * 5f;
        }
    }
}