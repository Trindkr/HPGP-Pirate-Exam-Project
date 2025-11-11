using Components;
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
            state.RequireForUpdate<PhysicsWorldSingleton>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var collisionWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>().CollisionWorld;
            float viewDistance = 10f;
            float collisionSphereRadius = 4f;

            var fleetMemberLookup = SystemAPI.GetComponentLookup<FleetMember>();

            ObstacleAvoidanceJob obstacleAvoidanceJob = new ObstacleAvoidanceJob
            {
                CollisionWorld = collisionWorld,
                AvoidanceForce = 100f,
                ViewDistance = viewDistance,
                CollisionSphereRadius = collisionSphereRadius,
                FleetMemberLookup = fleetMemberLookup,
            };
            state.Dependency = obstacleAvoidanceJob.Schedule(state.Dependency);
        }

        public partial struct ObstacleAvoidanceJob : IJobEntity
        {
            public float AvoidanceForce;
            public float ViewDistance;
            public float CollisionSphereRadius;

            public CollisionWorld CollisionWorld;
            [ReadOnly] public ComponentLookup<FleetMember> FleetMemberLookup;

            private void Execute(
                in LocalTransform localTransform,
                ref Navigation navigation,
                in FleetMember fleetMember)
            {
                // ReSharper disable once PossiblyImpureMethodCallOnReadonlyVariable
                // Forward does not mutate the struct
                var position = localTransform.Position;
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

                // var pointDistanceInput = new PointDistanceInput
                // {
                //     Filter = CollisionFilter.Default,
                //     MaxDistance = ViewDistance,
                //     Position = position
                // };

                // var hits = new NativeList<DistanceHit>();
                // var collector = new AllHitsCollector<DistanceHit>(ViewDistance, ref hits);
                //
                // if (!CollisionWorld.CalculateDistance(pointDistanceInput, ref collector))
                //     return;
                // Debug.Log(collector.NumHits);
                //
                // var closestDistance = float.MaxValue;
                // DistanceHit closestHit = default;
                // foreach (var distanceHit in collector.AllHits)
                // {
                //     if (distanceHit.Distance < 0.1f) continue;
                //     if (distanceHit.Distance < closestDistance)
                //     {
                //         closestDistance = distanceHit.Distance;
                //         closestHit = distanceHit;
                //     }
                // }

                // if (!CollisionWorld.CalculateDistance(pointDistanceInput, out DistanceHit closestHit))
                    // return;

                Debug.DrawLine(position, closestHit.Position, Color.magenta);
                var distanceToHit = math.distance(position, closestHit.Position);
                float normalizedDistance = math.saturate(1f - (distanceToHit / ViewDistance));

                float3 normal = closestHit.SurfaceNormal;
                float3 avoidanceDirection = math.normalize(math.reflect(localTransform.Forward(), normal));
                avoidanceDirection *= normalizedDistance * AvoidanceForce;

                navigation.DesiredDirection += avoidanceDirection;
                Debug.DrawLine(position, position + avoidanceDirection, Color.green);
                
                // hits.Dispose();
            }

            private static bool SameFleet(FleetMember fleetMember, FleetMember otherFleetMember)
            {
                return otherFleetMember.FleetEntity == fleetMember.FleetEntity;
            }
        }
    }
}