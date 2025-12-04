using Components;
using Components.Enum;
using Components.Fleet;
using Systems.Fleet;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;

namespace Systems
{
    [UpdateBefore(typeof(TurnSystem)), UpdateAfter(typeof(FleetFlockingSystem))]
    public partial struct ObstacleAvoidanceSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<JobModeSingleton>();
            state.RequireForUpdate<PhysicsWorldSingleton>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var collisionWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>().CollisionWorld;
            float viewDistance = 10f;
            float collisionSphereRadius = 4f;
            float avoidanceForce = 3f;
            var fleetMemberLookup = SystemAPI.GetComponentLookup<FleetMember>();
            
            ObstacleAvoidanceJob job = new ObstacleAvoidanceJob
            {
                CollisionWorld = collisionWorld,
                AvoidanceForce = avoidanceForce,
                ViewDistance = viewDistance,
                CollisionSphereRadius = collisionSphereRadius,
                FleetMemberLookup = fleetMemberLookup,
            };
            
            var jobModeSingleton = SystemAPI.GetSingleton<JobModeSingleton>();
            if (jobModeSingleton.JobMode == JobMode.Run)
            {
                foreach (var (localTransform, navigation, fleetMember, entity) 
                         in SystemAPI
                             .Query<RefRO<LocalTransform>, RefRW<Navigation>, RefRO<FleetMember>>()
                             .WithEntityAccess()
                             .WithNone<Sinking>())
                {
                    float3 forward = localTransform.ValueRO.Forward();
                    float3 verticalOffset = new float3(0f, 2f, 0);
                    float3 raycastStart = localTransform.ValueRO.Position + verticalOffset + forward * 2f;

                    if (!collisionWorld.SphereCast(
                            raycastStart,
                            collisionSphereRadius,
                            forward,
                            viewDistance,
                            out ColliderCastHit closestHit,
                            CollisionFilter.Default))
                        return;

                    if (fleetMemberLookup.TryGetComponent(closestHit.Entity, out var otherFleetMember)
                        && ObstacleAvoidanceJob.SameFleet(fleetMember.ValueRO, otherFleetMember))
                        return;

                    var distanceToHit = math.distance(raycastStart, closestHit.Position);
                    float normalizedDistance = math.saturate(1f - distanceToHit / viewDistance);

                
                    if (normalizedDistance < 0.2f)
                    {
                        navigation.ValueRW.DesiredMoveSpeed = 0f;
                    }
                    else
                    {
                        navigation.ValueRW.DesiredMoveSpeed *= normalizedDistance;
                    }

                    float3 normal = closestHit.SurfaceNormal;
                    float3 avoidanceDirection = math.normalize(math.reflect(forward, normal));
                    avoidanceDirection *= normalizedDistance * avoidanceForce;

                    navigation.ValueRW.DesiredDirection += avoidanceDirection;
                    Debug.DrawLine(raycastStart, raycastStart + avoidanceDirection, Color.green);
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
        
        [WithNone(typeof(Sinking))]
        public partial struct ObstacleAvoidanceJob : IJobEntity
        {
            public float AvoidanceForce;
            public float ViewDistance;
            public float CollisionSphereRadius;

            [ReadOnly] public CollisionWorld CollisionWorld;
            [ReadOnly] public ComponentLookup<FleetMember> FleetMemberLookup;

            private void Execute(
                in Entity entity,
                in LocalTransform localTransform,
                ref Navigation navigation,
                in FleetMember fleetMember)
            {
                // ReSharper disable once PossiblyImpureMethodCallOnReadonlyVariable
                // Forward does not mutate the struct
                float3 forward = localTransform.Forward();
                float3 verticalOffset = new float3(0f, 2f, 0);
                float3 raycastStart = localTransform.Position + verticalOffset + forward * 2f;

                if (!CollisionWorld.SphereCast(
                        raycastStart,
                        CollisionSphereRadius,
                        forward,
                        ViewDistance,
                        out ColliderCastHit closestHit,
                        CollisionFilter.Default))
                    return;

                if (FleetMemberLookup.TryGetComponent(closestHit.Entity, out var otherFleetMember)
                    && SameFleet(fleetMember, otherFleetMember))
                    return;

                var distanceToHit = math.distance(raycastStart, closestHit.Position);
                float normalizedDistance = math.saturate(1f - (distanceToHit / ViewDistance));

                
                if (normalizedDistance < 0.2f)
                {
                    navigation.DesiredMoveSpeed = 0f;
                }
                else
                {
                    navigation.DesiredMoveSpeed *= normalizedDistance;
                }

                float3 normal = closestHit.SurfaceNormal;
                float3 avoidanceDirection = math.normalize(math.reflect(forward, normal));
                avoidanceDirection *= normalizedDistance * AvoidanceForce;

                navigation.DesiredDirection += avoidanceDirection;
                Debug.DrawLine(raycastStart, raycastStart + avoidanceDirection, Color.green);
            }

            internal static bool SameFleet(FleetMember fleetMember, FleetMember otherFleetMember)
            {
                return otherFleetMember.FleetEntity == fleetMember.FleetEntity;
            }
        }
    }
}