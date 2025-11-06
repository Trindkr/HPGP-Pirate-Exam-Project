using Components.Fleet;
using ExtensionMethods;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Systems.DebugSystems
{
    public partial struct FleetDebugSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            float2 forward = new float2(0, 1);
            float2 back = new float2(0, -1);
            float2 left = new float2(-1, 0);
            float2 right = new float2(1, 0);
            
            foreach (var fleet in SystemAPI.Query<RefRO<Components.Fleet.Fleet>>())
            {
                Debug.DrawLine(fleet.ValueRO.Center.x0z(), fleet.ValueRO.Center.x0z() + forward.x0z(), Color.white);
                Debug.DrawLine(fleet.ValueRO.Center.x0z(), fleet.ValueRO.Center.x0z() + back.x0z(), Color.white);
                Debug.DrawLine(fleet.ValueRO.Center.x0z(), fleet.ValueRO.Center.x0z() + left.x0z(), Color.white);
                Debug.DrawLine(fleet.ValueRO.Center.x0z(), fleet.ValueRO.Center.x0z() + right.x0z(), Color.white);
                
                Debug.DrawLine(fleet.ValueRO.Center.x0z(), fleet.ValueRO.Center.x0z() + fleet.ValueRO.Alignment.x0z(), Color.white);
            }
        }
    }
}