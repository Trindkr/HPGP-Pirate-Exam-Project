using Components;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;

namespace Systems
{
    [BurstCompile]
    public partial struct MerchantSpawnerSystem : ISystem, ISystemStartStop
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>();
        }

        public void OnStartRunning(ref SystemState state)
        {
            var ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
            var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);
            SpawnBoats(ref state, ref ecb);
        }

        public void OnStopRunning(ref SystemState state)
        {
        }

        private void SpawnBoats(ref SystemState state, ref EntityCommandBuffer ecb)
        {
            foreach (var spawner in SystemAPI.Query<RefRO<MerchantSpawner>>())
            {
                ShipSpawnerHelper.SpawnBoats(
                    ref ecb, 
                    spawner.ValueRO.ShipPrefab,
                    spawner.ValueRO.SailingConstraints, 
                    spawner.ValueRO.CannonballPrefab,
                    spawner.ValueRO.CannonConstraints,
                    spawner.ValueRO.NumberOfShips, 
                    new uint2(0, 0));
            }
        }
    }
}