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
            foreach (var (localTransform, navigation, fleetMember) 
                     in SystemAPI.Query<RefRO<LocalTransform>, RefRW<Navigation>, RefRO<FleetMember>>())
            {
                float3 forward = localTransform.ValueRO.Forward();
                var raycast = new RaycastInput
                {
                    Start = localTransform.ValueRO.Position,
                    End = localTransform.ValueRO.Position + forward * viewDistance,
                    Filter = CollisionFilter.Default,
                };

                if (collisionWorld.CastRay(raycast, out RaycastHit closestHit))
                {
                    // Debug.DrawLine(
                    //     localTransform.ValueRO.Position, 
                    //     closestHit.Position, 
                    //     Color.green);
                }
                else
                {
                    // Debug.DrawLine(
                    //     localTransform.ValueRO.Position, 
                    //     localTransform.ValueRO.Position + forward * viewDistance,
                    //     Color.yellow);
                }
            }
        }
    }
}