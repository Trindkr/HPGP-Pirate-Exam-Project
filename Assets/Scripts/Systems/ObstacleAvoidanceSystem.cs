using Components;
using Components.Fleet;
using Systems.Fleet;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;
using RaycastHit = Unity.Physics.RaycastHit;

namespace Systems
{
    [UpdateAfter(typeof(FleetFlockingJob)), UpdateBefore(typeof(TurnSystem))]
    public partial struct ObstacleAvoidanceSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<PhysicsWorldSingleton>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var collisionWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>().CollisionWorld;
            float viewDistance = 10f;
            float collisionSphereRadius = 5f;

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
                if (!collisionWorld.SphereCast(
                        raycastStart, 
                        collisionSphereRadius, 
                        forward, 
                        viewDistance, 
                        out ColliderCastHit closestHit, 
                        CollisionFilter.Default))
                    continue;
                
                if (!fleetMemberLookup.TryGetComponent(closestHit.Entity, out var otherFleetMember)
                    || otherFleetMember.FleetEntity != fleetMember.ValueRO.FleetEntity)
                {
                    var distanceToHit = math.distance(raycastStart, closestHit.Position);
                    if (distanceToHit < 0.5f)
                        continue;
                    float normalizedDistance = math.saturate(1f - (distanceToHit / viewDistance));
                    
                    float3 normal = closestHit.SurfaceNormal;
                    float3 avoidanceDirection = math.normalize(math.reflect(forward, normal));
                    float avoidanceForce = 3f;
                    avoidanceDirection *= normalizedDistance * avoidanceForce;

                    navigation.ValueRW.DesiredDirection += avoidanceDirection;
                    Debug.DrawLine(raycastStart, raycastStart + avoidanceDirection, Color.green);
                }
            }
        }
    }
}