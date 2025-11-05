using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Physics;
using Unity.Physics.Systems;
using UnityEngine;

[UpdateInGroup(typeof(PhysicsSystemGroup))]
[UpdateAfter(typeof(PhysicsSimulationGroup))]
public partial struct CannonballCollisionSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<SimulationSingleton>();
    }

    public void OnUpdate(ref SystemState state)
    {
        var simulation = SystemAPI.GetSingleton<SimulationSingleton>().AsSimulation();
        var physicsWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>().PhysicsWorld;

        simulation.FinalJobHandle.Complete();

        var ecb = new EntityCommandBuffer(Allocator.Temp);

        foreach (var collisionEvent in simulation.CollisionEvents)
        {
            Debug.Log("Collision detected");

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

        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }

    private void HandleHit(ref EntityCommandBuffer ecb, Entity cannonball, Entity ship)
    {
        Debug.Log($"Cannonball {cannonball} hit Ship {ship}");

        ecb.AddComponent(ship, new Sinking());
        //ecb.DestroyEntity(cannonball); //should cannonball be destroyed?
    }
}
