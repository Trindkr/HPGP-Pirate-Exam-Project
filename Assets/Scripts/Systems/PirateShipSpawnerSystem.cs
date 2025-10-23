using Components;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Systems
{
    [BurstCompile]
    public partial struct PirateShipSpawnerSystem : ISystem, ISystemStartStop
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
            foreach (var spawner in SystemAPI.Query<RefRO<PirateShipSpawner>>())
            {
                for (uint j = 0; j < spawner.ValueRO.NumberOfShips; j++)
                {
                    for (uint i = 0; i < spawner.ValueRO.NumberOfShips; i++)
                    {
                        ShipSpawnerHelper.AddDefaultShipComponents(ecb, spawner.ValueRO.ShipPrefab, spawner.ValueRO.SailingConstraints, i+10, j+10);

                    }
                }
            }
        }
    }
}