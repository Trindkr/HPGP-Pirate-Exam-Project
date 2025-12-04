using Components;
using Components.Enum;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;
using UnityEngine;
using Random = Unity.Mathematics.Random;

namespace Systems.Cannon
{
    [UpdateInGroup(typeof(PhysicsSystemGroup))]
    [UpdateAfter(typeof(PhysicsSimulationGroup))]
    //[BurstCompile]
    public partial struct CannonballCollisionSystem : ISystem
    {
        //[BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<JobModeSingleton>();
            state.RequireForUpdate<SimulationSingleton>();
            state.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();
        }

        //[BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var simulationSingleton = SystemAPI.GetSingleton<SimulationSingleton>();
            var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
            var jobModeSingleton = SystemAPI.GetSingleton<JobModeSingleton>();

            if (jobModeSingleton.JobMode is JobMode.Schedule or JobMode.ScheduleParallel)
            {
                var job = new CannonballCollisionJob
                {
                    CannonballLookup = SystemAPI.GetComponentLookup<CannonballTag>(true),
                    ShipLookup = SystemAPI.GetComponentLookup<Ship>(true),
                    Ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter()
                };

                state.Dependency = job.Schedule(simulationSingleton, state.Dependency);
            }

            else if (jobModeSingleton.JobMode == JobMode.Run)
            {
                var simulation = simulationSingleton.AsSimulation();
                simulation.FinalJobHandle.Complete();

                var ecb = new EntityCommandBuffer(Allocator.Temp);

                foreach (var collisionEvent in simulation.CollisionEvents)
                {
                    Entity entityA = collisionEvent.EntityA;
                    Entity entityB = collisionEvent.EntityB;

                    bool aIsCannonball = SystemAPI.HasComponent<CannonballTag>(entityA);
                    bool bIsCannonball = SystemAPI.HasComponent<CannonballTag>(entityB);

                    bool aIsShip = SystemAPI.HasComponent<Ship>(entityA);
                    bool bIsShip = SystemAPI.HasComponent<Ship>(entityB);

                    if (aIsCannonball && bIsShip)
                    {
                        HandleHit(ref ecb, entityA, entityB);
                    }
                    else if (bIsCannonball && aIsShip)
                    {
                        HandleHit(ref ecb, entityB, entityA);
                    }
                }
            }
                
        }

        private static void HandleHit(ref EntityCommandBuffer ecb, Entity cannonball, Entity ship)
        {
            var rand = new Random((uint)(ship.Index * 7919 + cannonball.Index * 17));

            ecb.AddComponent(ship, new Sinking
            {
                SinkSpeed = 1f,
                TiltSpeed = 20f,
                TiltAxis = math.normalize(new float3(
                    rand.NextFloat(-1f, 1f),
                    0f,
                    rand.NextFloat(-1f, 1f)))
            });

            ecb.DestroyEntity(cannonball);
        }

        //[BurstCompile]
        private struct CannonballCollisionJob : ICollisionEventsJob
        {
            [ReadOnly] public ComponentLookup<CannonballTag> CannonballLookup;
            [ReadOnly] public ComponentLookup<Ship> ShipLookup;
            public EntityCommandBuffer.ParallelWriter Ecb;

            public void Execute(CollisionEvent collisionEvent)
            {
                var entityA = collisionEvent.EntityA;
                var entityB = collisionEvent.EntityB;

                bool aIsCannonball = CannonballLookup.HasComponent(entityA);
                bool bIsCannonball = CannonballLookup.HasComponent(entityB);
                bool aIsShip = ShipLookup.HasComponent(entityA);
                bool bIsShip = ShipLookup.HasComponent(entityB);

                if (aIsCannonball && bIsShip)
                {
                    HandleHit(entityA, entityB);
                }
                else if (bIsCannonball && aIsShip)
                {
                    HandleHit(entityB, entityA);
                }
            }

            private void HandleHit(Entity cannonball, Entity ship)
            {
                var rand = new Random((uint)(ship.Index * 7919 + cannonball.Index * 17));

                Ecb.AddComponent(0, ship, new Sinking
                {
                    SinkSpeed = 1f,
                    TiltSpeed = 20f,
                    TiltAxis = math.normalize(new float3(
                        rand.NextFloat(-1f, 1f),
                        0f,
                        rand.NextFloat(-1f, 1f)))
                });

                Ecb.DestroyEntity(0, cannonball);
            }
        }
    }
}
