using Components;
using ExtensionMethods;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Systems
{
    public partial struct IslandSpawnerSystem : ISystem, ISystemStartStop
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>();
        }

        public void OnStartRunning(ref SystemState state)
        {
            var ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
            var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);

            foreach (var islandSpawner in SystemAPI.Query<RefRO<IslandSpawner>>())
            {
                for (int i = 0; i < islandSpawner.ValueRO.IslandAmount; i++)
                {
                    float angle = math.PI2 / islandSpawner.ValueRO.IslandAmount * i;
                    float2 pointOnUnitCircle = new float2(math.cos(angle), math.sin(angle));
                    float2 spawnPoint = pointOnUnitCircle * islandSpawner.ValueRO.Radius;
                    Entity island = ecb.Instantiate(islandSpawner.ValueRO.IslandPrefab);
                    var localTransform =
                        LocalTransform.FromPosition(spawnPoint.x0z());
                    ecb.SetComponent(island, localTransform);
                }
            }
        }

        public void OnStopRunning(ref SystemState state)
        {
        }
    }
}