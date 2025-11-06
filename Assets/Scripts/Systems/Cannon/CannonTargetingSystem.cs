using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;
using Components.Cannon;
using Components.Enum;

using RaycastHit = Unity.Physics.RaycastHit;

namespace Systems.Cannon
{
    [BurstCompile]
    public partial struct CannonTargetingSystem : ISystem
    {
        private uint _pirateLayerMask;
        private uint _merchantLayerMask;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<PhysicsWorldSingleton>();

            // LayerMask.NameToLayer must be done outside Burst
            _pirateLayerMask = 1u << LayerMask.NameToLayer("Pirate");
            _merchantLayerMask = 1u << LayerMask.NameToLayer("Merchant");
        }
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var physicsWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>().CollisionWorld;

            var raycastYOffset = new float3(0, .5f, 0); 

            foreach (var (localToWorld, cannonConstraints, faction) in
                     SystemAPI.Query<RefRO<LocalToWorld>, RefRW<CannonConstraints>, RefRO<Faction>>())
            {
                float3 raycastStartPosition = localToWorld.ValueRO.Position + raycastYOffset;
                float3 rightDirection = localToWorld.ValueRO.Right;
                float3 leftDirection = -rightDirection;
                float range = cannonConstraints.ValueRO.ShootingRange;

                uint targetMask = faction.ValueRO.Value == FactionType.Pirate ? _merchantLayerMask : _pirateLayerMask;

                var filter = new CollisionFilter
                {
                    BelongsTo = ~0u,
                    CollidesWith = targetMask,
                    GroupIndex = 0
                };

                // Right ray
                var rightRayEnd = raycastStartPosition + rightDirection * range;
                var rightRay = new RaycastInput
                {
                    Start = raycastStartPosition,
                    End = rightRayEnd,
                    Filter = filter
                };

                if (physicsWorld.CastRay(rightRay, out RaycastHit hitRight))
                {
                    cannonConstraints.ValueRW.ShootingDirection = ShootingDirection.Right;
                    Debug.DrawLine(raycastStartPosition, hitRight.Position, Color.green);
                    continue;
                }
               Debug.DrawLine(raycastStartPosition, rightRayEnd, Color.red);

                // Left ray
                var leftRayEnd = raycastStartPosition + leftDirection * range;
                var leftRay = new RaycastInput
                {
                    Start = raycastStartPosition,
                    End = leftRayEnd,
                    Filter = filter
                };

                if (physicsWorld.CastRay(leftRay, out RaycastHit hitLeft))
                {
                    cannonConstraints.ValueRW.ShootingDirection = ShootingDirection.Left;
                    Debug.DrawLine(raycastStartPosition, hitLeft.Position, Color.green);
                }
                else
                {
                    cannonConstraints.ValueRW.ShootingDirection = ShootingDirection.None;
                    Debug.DrawLine(raycastStartPosition, leftRayEnd, Color.red);
                }
            }
        }
    }
}
