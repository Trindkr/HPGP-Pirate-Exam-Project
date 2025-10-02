using Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Systems
{
    [BurstCompile]
    public partial struct ShipSpawnerSystem : ISystem, ISystemStartStop
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();
        }

        public void OnStartRunning(ref SystemState state)
        {
            var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
            var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);
            SpawnBoats(ref state, ref ecb);
        }

        public void OnStopRunning(ref SystemState state)
        {
        }

        private void SpawnBoats(ref SystemState state, ref EntityCommandBuffer ecb)
        {
            foreach (var spawner in SystemAPI.Query<RefRO<ShipSpawner>>())
            {
                for (int i = 0; i < spawner.ValueRO.NumberOfShips; i++)
                {
                    var ship = ecb.Instantiate(spawner.ValueRO.ShipPrefab);
                    ecb.AddComponent(ship, new Ship());

                    var localTransform = LocalTransform.FromPosition(new float3(i * 5, 0, 0));
                    ecb.SetComponent(ship, localTransform);
                }
            }
        }
    }
}