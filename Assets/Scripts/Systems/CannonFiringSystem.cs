using Components;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

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
                    return;

                cannonConstraints.ValueRW.ReloadTimer = cannonConstraints.ValueRO.ReloadTime;

                var cannonball = EntityCommandBuffer.Instantiate(chunkIndex, cannonballPrefab.Prefab);

                var right = math.normalize(math.cross(shipTransform.Up, shipTransform.Forward));
                var shootDir = cannonConstraints.ValueRO.FireLeft ? -right : right;

                cannonConstraints.ValueRW.FireLeft = !cannonConstraints.ValueRO.FireLeft;

                const float upwardAngleDegrees = 30f;
                var rotationAxis = math.cross(shootDir, shipTransform.Up);
                shootDir = math.normalize(math.rotate(quaternion.AxisAngle(rotationAxis, math.radians(upwardAngleDegrees)), shootDir));

                var spawnPosition = shipTransform.Position + shootDir * 1.5f;

                EntityCommandBuffer.SetComponent(chunkIndex, cannonball, new LocalTransform
                {
                    Position = spawnPosition,
                    Rotation = quaternion.LookRotationSafe(shootDir, shipTransform.Up),
                    Scale = 1f
                });

                EntityCommandBuffer.SetComponent(chunkIndex, cannonball, new Unity.Physics.PhysicsVelocity
                {
                    Linear = shootDir * cannonConstraints.ValueRO.ShootingForce,
                    Angular = float3.zero
                });
            }

        }
    }
}