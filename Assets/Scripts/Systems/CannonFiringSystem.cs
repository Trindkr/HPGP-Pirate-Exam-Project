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
                RefRW<Cannon> cannon,
                in LocalToWorld shipTransform,
                in CannonballPrefab cannonballPrefab)
            {

                // TODO:
                // 1. Count down the reload timer for the cannon, reset when it reaches zero.
                // 3. Instantiate the cannonball prefab.
                // 4. Compute the cannon's forward direction and spawn position
                //      (spawn position is ships posision + some offset perhaps. And the forward direction depends on whether firing left or right, but is a vector perpendicular to the ships forward vector).
                // 5. Apply an initial PhysicsVelocity to the cannonball based on ShootingForce.


                // test if instantiation works
                cannon.ValueRW.ReloadTimer -= DeltaTime;

                if (cannon.ValueRW.ReloadTimer > 0f)
                {
                    return;
                }

                cannon.ValueRW.ReloadTimer = cannon.ValueRO.ReloadTime;
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
