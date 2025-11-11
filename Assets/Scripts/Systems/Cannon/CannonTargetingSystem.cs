using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;
using Components.Cannon;
using Components.Enum;

using RaycastHit = Unity.Physics.RaycastHit;
using Unity.Collections;

namespace Systems.Cannon
{
    [BurstCompile]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial struct CannonTargetingSystem : ISystem
    {
        private uint _pirateLayerMask;
        private uint _merchantLayerMask;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<PhysicsWorldSingleton>();

            _pirateLayerMask = 1u << LayerMask.NameToLayer("Pirate");
            _merchantLayerMask = 1u << LayerMask.NameToLayer("Merchant");
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var physicsWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>();
            var job = new CannonTargetingJob
            {
                CollisionWorld = physicsWorld.CollisionWorld,
                PirateMask = _pirateLayerMask,
                MerchantMask = _merchantLayerMask,
                RaycastYOffset = new float3(0, 0.5f, 0)
            };

            state.Dependency = job.ScheduleParallel(state.Dependency);
        }

        [BurstCompile]
        private partial struct CannonTargetingJob : IJobEntity
        {
            [ReadOnly] public CollisionWorld CollisionWorld;
            [ReadOnly] public uint PirateMask;
            [ReadOnly] public uint MerchantMask;
            public float3 RaycastYOffset;

            //TODO: fjern evt debug drawlines?
            void Execute(ref CannonConstraints cannonConstraints, in LocalToWorld localToWorld, in Faction faction)
            {
                float3 start = localToWorld.Position + RaycastYOffset;
                float3 right = localToWorld.Right;
                float3 left = -right;
                float range = cannonConstraints.ShootingRange;

                uint targetMask = faction.Value == FactionType.Pirate ? MerchantMask : PirateMask;
                float tolerance = range * 0.8f;

                var filter = new CollisionFilter
                {
                    BelongsTo = ~0u,
                    CollidesWith = targetMask,
                    GroupIndex = 0
                };

                bool foundTarget = false;

                // right ray
                var rightRay = new RaycastInput
                {
                    Start = start,
                    End = start + right * range,
                    Filter = filter
                };

                if (CollisionWorld.CastRay(rightRay, out RaycastHit hitRight))
                {
#if UNITY_EDITOR
                    Debug.DrawLine(start, hitRight.Position, Color.green);
#endif
                    float hitDistance = math.distance(hitRight.Position, start);
                    if (hitDistance >= tolerance)
                    {
                        cannonConstraints.ShootingDirection = ShootingDirection.Right;
                        foundTarget = true;
                    }
                }
#if UNITY_EDITOR
                else
                {
                    Debug.DrawLine(start, start + right * range, Color.red);
                }
#endif

                if (foundTarget)
                    return;

                // left ray
                var leftRay = new RaycastInput
                {
                    Start = start,
                    End = start + left * range,
                    Filter = filter
                };

                if (CollisionWorld.CastRay(leftRay, out RaycastHit hitLeft))
                {
#if UNITY_EDITOR
                    Debug.DrawLine(start, hitLeft.Position, Color.green);
#endif
                    float hitDistance = math.distance(hitLeft.Position, start);
                    if (hitDistance >= tolerance)
                    {
                        cannonConstraints.ShootingDirection = ShootingDirection.Left;
                        foundTarget = true;
                    }
                }
#if UNITY_EDITOR
                else
                {
                    Debug.DrawLine(start, start + left * range, Color.red);
                }
#endif

                if (!foundTarget)
                {
                    cannonConstraints.ShootingDirection = ShootingDirection.None;
                }
            }
        }
    }
}
