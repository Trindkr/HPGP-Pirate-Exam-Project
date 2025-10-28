using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Systems
{
    [BurstCompile]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial struct CannonFiringSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            float deltaTime = SystemAPI.Time.DeltaTime;

            var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
            var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();

            var job = new CannonFireJob
            {
                DeltaTime = deltaTime,
                EntityCommandBuffer = ecb
            };

            state.Dependency = job.ScheduleParallel(state.Dependency);
        }

        [BurstCompile]
        public partial struct CannonFireJob : IJobEntity
        {
            public float DeltaTime;
            public EntityCommandBuffer.ParallelWriter EntityCommandBuffer;

            public void Execute(
                [ChunkIndexInQuery] int chunkIndex,
                RefRW<CannonConstraints> cannonConstraints,
                in LocalToWorld shipTransform,
                in CannonballPrefab cannonballPrefab)
            {
                cannonConstraints.ValueRW.ReloadTimer -= DeltaTime;

                if (cannonConstraints.ValueRW.ReloadTimer > 0f)
                {
                    return;
                }

                cannonConstraints.ValueRW.ReloadTimer = cannonConstraints.ValueRO.ReloadTime;
                var cannonball = EntityCommandBuffer.Instantiate(chunkIndex, cannonballPrefab.Prefab);

                float3 spawnPosition = shipTransform.Position;

                EntityCommandBuffer.SetComponent(chunkIndex, cannonball, new LocalTransform
                {
                    Position = spawnPosition,
                    Rotation = quaternion.identity,
                    Scale = 1f
                });
            }
        }
    }
}
