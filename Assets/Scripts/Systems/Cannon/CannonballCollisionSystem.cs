using Components;
using Components.Enum;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;

namespace Systems.Cannon
{
    [UpdateInGroup(typeof(PhysicsSystemGroup))]
    [UpdateAfter(typeof(PhysicsSimulationGroup))]
    [BurstCompile]
    public partial struct CannonballCollisionSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<SimulationSingleton>();
            state.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var simulation = SystemAPI.GetSingleton<SimulationSingleton>();
            var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
        
            var job = new CannonballCollisionJob
            {
                CannonballLookup = SystemAPI.GetComponentLookup<CannonballTag>(true),
                ShipLookup = SystemAPI.GetComponentLookup<Ship>(true),
                Ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter()
            };

            state.Dependency = job.Schedule(simulation, state.Dependency);
        }


        [BurstCompile]
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
