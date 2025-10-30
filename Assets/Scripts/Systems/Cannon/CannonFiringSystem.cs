using Components.Cannon;
using Components.Enum;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Systems.Cannon
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
                ref CannonConstraints cannonConstraints,
                in LocalToWorld shipTransform,
                in CannonballPrefab cannonballPrefab)
            {
                cannonConstraints.ReloadTimer -= DeltaTime;
                if (cannonConstraints.ReloadTimer > 0f)
                    return;

                cannonConstraints.ReloadTimer = cannonConstraints.ReloadTime;

                if (cannonConstraints.ShootingDirection == ShootingDirection.None)
                    return;

                var cannonball = EntityCommandBuffer.Instantiate(chunkIndex, cannonballPrefab.Prefab);

                var right = math.normalize(math.cross(shipTransform.Up, shipTransform.Forward));
                var shootDir = cannonConstraints.ShootingDirection switch
                {
                    ShootingDirection.Left => -right,
                    ShootingDirection.Right => right,
                    _ => float3.zero
                };

                var rotationAxis = math.cross(shootDir, shipTransform.Up);
                shootDir = math.normalize(math.rotate(
                    quaternion.AxisAngle(rotationAxis, math.radians(cannonConstraints.ShootingAngle)),
                    shootDir));

                var spawnPosition = shipTransform.Position + shootDir * 1.5f;

                EntityCommandBuffer.SetComponent(chunkIndex, cannonball, new LocalTransform
                {
                    Position = spawnPosition,
                    Rotation = quaternion.LookRotationSafe(shootDir, shipTransform.Up),
                    Scale = 1f
                });

                EntityCommandBuffer.SetComponent(chunkIndex, cannonball, new Unity.Physics.PhysicsVelocity
                {
                    Linear = shootDir * cannonConstraints.ShootingForce,
                    Angular = float3.zero
                });
            }

        }
    }
}