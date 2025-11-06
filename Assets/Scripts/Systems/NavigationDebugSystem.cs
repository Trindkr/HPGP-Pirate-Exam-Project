using System;
using Components;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Systems
{
    [BurstCompile, UpdateAfter(typeof(MoveSystem))]
    public partial struct NavigationDebugSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            foreach (var (transform, navigation) in SystemAPI.Query<RefRO<LocalTransform>, RefRO<Navigation>>())
            {
                var position = transform.ValueRO.Position;
                var desiredDirection = navigation.ValueRO.DesiredDirection;
                Debug.DrawLine(position, position + desiredDirection, Color.red);
                
                // var forward = transform.ValueRO.Forward();
                // var speed = navigation.ValueRO.DesiredMoveSpeed;
                // Debug.DrawLine(position, position + forward * speed, Color.yellow);
                //
                // var pointAroundTransform = new float3(Mathf.Cos((float)SystemAPI.Time.ElapsedTime * 5f), 0, Mathf.Sin((float)SystemAPI.Time.ElapsedTime * 5f));
                // float radius = math.sqrt(500);
                // Debug.DrawLine(position, position + pointAroundTransform * radius, Color.green);
            }
        }
    }
}