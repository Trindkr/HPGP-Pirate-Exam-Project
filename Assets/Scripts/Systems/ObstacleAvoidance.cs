using Components;
using Components.Fleet;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;
using RaycastHit = Unity.Physics.RaycastHit;

namespace Systems
{
    public partial struct ObstacleAvoidance : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<PhysicsWorldSingleton>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var collisionWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>().CollisionWorld;
            float viewDistance = 20f;

            var fleetMemberLookup = SystemAPI.GetComponentLookup<FleetMember>();
            
            foreach (var (localTransform, navigation, fleetMember) 
                     in SystemAPI.Query<RefRO<LocalTransform>, RefRW<Navigation>, RefRO<FleetMember>>())
            {
                // ReSharper disable once PossiblyImpureMethodCallOnReadonlyVariable
                // Forward does not mutate the struct
                float3 forward = localTransform.ValueRO.Forward();
                float3 verticalOffset = new float3(0f, 2f, 0);
                float3 raycastStart = localTransform.ValueRO.Position + verticalOffset;
                float3 raycastEnd = raycastStart + forward * viewDistance;
                var raycast = new RaycastInput
                {
                    Start = raycastStart,
                    End = raycastEnd,
                    Filter = CollisionFilter.Default,
                };

                // Possibly make this a sphere cast for wider collision detection
                if (collisionWorld.CastRay(raycast, out RaycastHit closestHit))
                {
                    // Has a fleet component
                    if (fleetMemberLookup.TryGetComponent(closestHit.Entity, out var otherFleetMember))
                    {
                        if (otherFleetMember.FleetEntity == fleetMember.ValueRO.FleetEntity)
                        {
                           // Is same fleet - Do not use avoidance
                            Debug.DrawLine(
                                raycastStart, 
                                closestHit.Position, 
                                Color.red);
                            continue;
                        }
                    }
                    // Hit something that is not part of the fleet, should avoid
                    Debug.DrawLine(
                        raycastStart, 
                        closestHit.Position, 
                        Color.green);
                    continue;
                }
                // Hit nothing
                Debug.DrawLine(
                    raycastStart, 
                    raycastEnd,
                    Color.yellow);
            }
        }
    }
}