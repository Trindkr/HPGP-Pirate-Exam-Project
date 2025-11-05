using Components.Cannon;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Systems.Cannon
{
    [BurstCompile]
    public partial struct ShootingDebugSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            //foreach (var (transform, cannonConstraints) in SystemAPI
            //             .Query<RefRO<LocalTransform>, RefRO<CannonConstraints>>())
            //{
            //    var position = transform.ValueRO.Position;
            //    var shootingRange = cannonConstraints.ValueRO.ShootingRange;

            //    var pointAroundTransform = new float3(Mathf.Cos((float)SystemAPI.Time.ElapsedTime * 5f), 0,
            //        Mathf.Sin((float)SystemAPI.Time.ElapsedTime * 5f));

            //    Debug.DrawLine(position, position + pointAroundTransform * shootingRange, Color.blueViolet);
            //}
        }
    }
}