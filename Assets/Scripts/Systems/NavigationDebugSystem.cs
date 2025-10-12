using System;
using Components;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace Systems
{
    public partial struct NavigationDebugSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            foreach (var (transform, navigation) in SystemAPI.Query<RefRO<LocalTransform>, RefRO<Navigation>>())
            {
                var position = transform.ValueRO.Position;
                var desiredDirection = navigation.ValueRO.DesiredDirection;
                Debug.DrawLine(position, position + desiredDirection, Color.red);

                var forward = transform.ValueRO.Forward();
                var speed = navigation.ValueRO.DesiredSpeed;
                Debug.DrawLine(position, position + forward * speed, Color.yellow);
            }
        }
    }
}