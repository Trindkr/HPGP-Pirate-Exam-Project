using Components;
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
    public partial struct CannonTargetingSystem : ISystem
    {
        private uint _pirateLayerMask;
        private uint _merchantLayerMask;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<JobModeSingleton>();
            state.RequireForUpdate<PhysicsWorldSingleton>();

            _pirateLayerMask = 1u << LayerMask.NameToLayer("Pirate");
            _merchantLayerMask = 1u << LayerMask.NameToLayer("Merchant");
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var raycastYOffset = new float3(0, 0.5f, 0);
            
            var physicsWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>();
            var job = new CannonTargetingJob
            {
                CollisionWorld = physicsWorld.CollisionWorld,
                PirateMask = _pirateLayerMask,
                MerchantMask = _merchantLayerMask,
                RaycastYOffset = raycastYOffset
            };

            var jobModeSingleton = SystemAPI.GetSingleton<JobModeSingleton>();
            if (jobModeSingleton.JobMode == JobMode.Run)
            {
                state.Dependency.Complete();
                
                foreach (
                    var (cannonConstraints, localTransform, faction) 
                         in SystemAPI.Query<RefRW<CannonConstraints>, RefRO<LocalTransform>, RefRO<Faction>>())
                {
                    float3 start = localTransform.ValueRO.Position + raycastYOffset;
                    float3 right = localTransform.ValueRO.Right();
                    float3 left = -right;
                    float range = cannonConstraints.ValueRO.ShootingRange;

                    uint targetMask = faction.ValueRO.Value == FactionType.Pirate ? _merchantLayerMask : _pirateLayerMask;
                    float tolerance = range * 0f;

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

                    if (physicsWorld.CollisionWorld.CastRay(rightRay, out RaycastHit hitRight))
                    {
    #if UNITY_EDITOR
                        Debug.DrawLine(start, hitRight.Position, Color.green);
    #endif
                        float hitDistance = math.distance(hitRight.Position, start);
                        if (hitDistance >= tolerance)
                        {
                            cannonConstraints.ValueRW.ShootingDirection = ShootingDirection.Right;
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

                    if (physicsWorld.CollisionWorld.CastRay(leftRay, out RaycastHit hitLeft))
                    {
    #if UNITY_EDITOR
                         Debug.DrawLine(start, hitLeft.Position, Color.green);
    #endif
                        float hitDistance = math.distance(hitLeft.Position, start);
                        if (hitDistance >= tolerance)
                        {
                            Debug.Log("CannonTargetingSystem: Target found");
                            cannonConstraints.ValueRW.ShootingDirection = ShootingDirection.Left;
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
                        cannonConstraints.ValueRW.ShootingDirection = ShootingDirection.None;
                    }
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

        [BurstCompile]
        private partial struct CannonTargetingJob : IJobEntity
        {
            [ReadOnly] public CollisionWorld CollisionWorld;
            [ReadOnly] public uint PirateMask;
            [ReadOnly] public uint MerchantMask;
            public float3 RaycastYOffset;

            void Execute(ref CannonConstraints cannonConstraints, in LocalTransform localTransform, in Faction faction)
            {
                float3 start = localTransform.Position + RaycastYOffset;
                float3 right = localTransform.Right();
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
                    // Debug.DrawLine(start, hitRight.Position, Color.green);
#endif
                    float hitDistance = math.distance(hitRight.Position, start);
                    if (hitDistance >= tolerance)
                    {
                        cannonConstraints.ShootingDirection = ShootingDirection.Right;
                        foundTarget = true;
                    }
                }
#if UNITY_EDITOR
                // else
                // {
                //     Debug.DrawLine(start, start + right * range, Color.red);
                // }
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
                    // Debug.DrawLine(start, hitLeft.Position, Color.green);
#endif
                    float hitDistance = math.distance(hitLeft.Position, start);
                    if (hitDistance >= tolerance)
                    {
                        cannonConstraints.ShootingDirection = ShootingDirection.Left;
                        foundTarget = true;
                    }
                }
#if UNITY_EDITOR
                // else
                // {
                //     Debug.DrawLine(start, start + left * range, Color.red);
                // }
#endif

                if (!foundTarget)
                {
                    cannonConstraints.ShootingDirection = ShootingDirection.None;
                }
            }
        }
    }
}
