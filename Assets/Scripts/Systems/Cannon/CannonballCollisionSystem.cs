using Unity.Collections;
using Unity.Entities;
using Unity.Physics;
using Unity.Physics.Systems;
using UnityEngine;

namespace Systems.Cannon
{
    [UpdateInGroup(typeof(PhysicsSystemGroup))]
    [UpdateAfter(typeof(PhysicsSimulationGroup))]
    public partial struct CannonballCollisionSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();
            state.RequireForUpdate<SimulationSingleton>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var simulation = SystemAPI.GetSingleton<SimulationSingleton>().AsSimulation();
            

            simulation.FinalJobHandle.Complete();

            var ecbSystem = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
            var ecb = ecbSystem.CreateCommandBuffer(state.WorldUnmanaged);

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

        private void HandleHit(ref EntityCommandBuffer ecb, Entity cannonball, Entity ship)
        {
            ecb.AddComponent(ship, new Sinking());
            ecb.DestroyEntity(cannonball); //should cannonball be destroyed?
        }
    }
}
