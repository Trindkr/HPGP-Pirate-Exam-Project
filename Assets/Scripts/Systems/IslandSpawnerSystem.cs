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

            // Create singleton entity
            var singleton = state.EntityManager.CreateEntity();
            state.EntityManager.AddBuffer<IslandPositionBuffer>(singleton);
            DynamicBuffer<IslandPositionBuffer> buffer = state.EntityManager.GetBuffer<IslandPositionBuffer>(singleton);
            
            foreach (var islandSpawner in SystemAPI.Query<RefRO<IslandSpawner>>())
            {
                for (int i = 0; i < islandSpawner.ValueRO.IslandAmount; i++)
                {
                    float angle = math.PI2 / islandSpawner.ValueRO.IslandAmount * i;
                    var pointOnUnitCircle = new float2(math.cos(angle), math.sin(angle));
                    var spawnPoint = pointOnUnitCircle * islandSpawner.ValueRO.Radius;
                    var position = spawnPoint.x0z(); 
                    
                    Entity island = ecb.Instantiate(islandSpawner.ValueRO.IslandPrefab);
                    var localTransform =
                        LocalTransform.FromPosition(position);
                    ecb.SetComponent(island, localTransform);

                    buffer.Add(new IslandPositionBuffer
                    {
                        Position = position,
                    });
                }
            }
        }

        public void OnStopRunning(ref SystemState state)
        {
        }
    }
}