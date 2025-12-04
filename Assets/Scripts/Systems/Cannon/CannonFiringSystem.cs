using Components;
using Components.Cannon;
using Components.Enum;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Systems.Cannon
{
    [BurstCompile]
    [UpdateAfter(typeof(CannonTargetingSystem))]
    public partial struct CannonFiringSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<JobModeSingleton>();
            state.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var deltaTime = SystemAPI.Time.DeltaTime;


            var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
            var ecbParallel = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();
            
            var job = new CannonFireJob
            {
                DeltaTime = deltaTime,
                EntityCommandBuffer = ecbParallel
            };

            
            var jobModeSingleton = SystemAPI.GetSingleton<JobModeSingleton>();
            if (jobModeSingleton.JobMode == JobMode.Run)
            {
                var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);
                foreach (
                    var (cannonConstraints, shipTransform, cannonballPrefab)
                    in SystemAPI.Query<RefRW<CannonConstraints>, RefRO<LocalTransform>, RefRO<CannonballPrefab>>())
                {
                    cannonConstraints.ValueRW.ReloadTimer -= deltaTime;
                    //Debug.Log("Reloadtimer: " + cannonConstraints.ValueRO.ReloadTimer+", Deltatime:"+deltaTime);
                    
                     if (cannonConstraints.ValueRO.ReloadTimer > 0f)
                         return;
                    //Debug.Log("Has no reload time.");

                     if (cannonConstraints.ValueRO.ShootingDirection == ShootingDirection.None)
                         return;
                     //Debug.Log("Has shooting direction");
                    
                    //cannonConstraints.ValueRW.ShootingDirection = ShootingDirection.Left;
                    var cannonball = ecb.Instantiate(cannonballPrefab.ValueRO.Prefab);

                    var right = math.normalize(math.cross(shipTransform.ValueRO.Up(), shipTransform.ValueRO.Forward()));
                    var shootDir = cannonConstraints.ValueRO.ShootingDirection switch
                    {
                        ShootingDirection.Left => -right,
                        ShootingDirection.Right => right,
                        _ => float3.zero
                    };

                    var rotationAxis = math.cross(shootDir, shipTransform.ValueRO.Up());
                    shootDir = math.normalize(math.rotate(
                        quaternion.AxisAngle(rotationAxis, math.radians(cannonConstraints.ValueRO.ShootingAngle)),
                        shootDir));

                    var spawnPosition = shipTransform.ValueRO.Position + (shootDir * 5f);

                    ecb.SetComponent(cannonball, new LocalTransform
                    {
                        Position = spawnPosition,
                        Rotation = quaternion.LookRotationSafe(shootDir, shipTransform.ValueRO.Up()),
                        Scale = 1f
                    });

                    ecb.SetComponent(cannonball, new Unity.Physics.PhysicsVelocity
                    {
                        Linear = shootDir * cannonConstraints.ValueRO.ShootingForce,
                        Angular = float3.zero
                    });
                
                    cannonConstraints.ValueRW.ReloadTimer = cannonConstraints.ValueRO.ReloadTime;
                }
            }
            else if (jobModeSingleton.JobMode == JobMode.Schedule)
            {
                state.Dependency = job.Schedule(state.Dependency);
            }
            else
            {
                state.Dependency = job.ScheduleParallel(state.Dependency);
            }
            
        }

        [BurstCompile]
        public partial struct CannonFireJob : IJobEntity
        {
            public float DeltaTime;
            public EntityCommandBuffer.ParallelWriter EntityCommandBuffer;


            private void Execute(
                [ChunkIndexInQuery] int chunkIndex,
                ref CannonConstraints cannonConstraints,
                in LocalTransform shipTransform,
                in CannonballPrefab cannonballPrefab)
            {
                cannonConstraints.ReloadTimer -= DeltaTime;
                if (cannonConstraints.ReloadTimer > 0f)
                    return;

                if (cannonConstraints.ShootingDirection == ShootingDirection.None)
                    return;

                var cannonball = EntityCommandBuffer.Instantiate(chunkIndex, cannonballPrefab.Prefab);

                var right = math.normalize(math.cross(shipTransform.Up(), shipTransform.Forward()));
                var shootDir = cannonConstraints.ShootingDirection switch
                {
                    ShootingDirection.Left => -right,
                    ShootingDirection.Right => right,
                    _ => float3.zero
                };

                var rotationAxis = math.cross(shootDir, shipTransform.Up());
                shootDir = math.normalize(math.rotate(
                    quaternion.AxisAngle(rotationAxis, math.radians(cannonConstraints.ShootingAngle)),
                    shootDir));

                var spawnPosition = shipTransform.Position + (shootDir * 5f);

                EntityCommandBuffer.SetComponent(chunkIndex, cannonball, new LocalTransform
                {
                    Position = spawnPosition,
                    Rotation = quaternion.LookRotationSafe(shootDir, shipTransform.Up()),
                    Scale = 1f
                });

                EntityCommandBuffer.SetComponent(chunkIndex, cannonball, new Unity.Physics.PhysicsVelocity
                {
                    Linear = shootDir * cannonConstraints.ShootingForce,
                    Angular = float3.zero
                });
                
                cannonConstraints.ReloadTimer = cannonConstraints.ReloadTime;
            }

        }
    }
}