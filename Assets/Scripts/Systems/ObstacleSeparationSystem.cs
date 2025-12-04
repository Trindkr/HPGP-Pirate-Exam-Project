using Components;
using Components.Enum;
using Systems.Fleet;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.SocialPlatforms;

namespace Systems
{
    [BurstCompile]
    [UpdateBefore(typeof(TurnSystem)), UpdateAfter(typeof(FleetFlockingSystem))]
    public partial struct ObstacleSeparationSystem : ISystem
    {
        private EntityQuery _query;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<JobModeSingleton>();
            _query =
                SystemAPI.QueryBuilder()
                    .WithAll<Ship>()
                    .WithNone<Sinking>()
                    .WithAll<LocalTransform>().Build();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var transformLookup = SystemAPI.GetComponentLookup<LocalTransform>(true);
            var ships = _query.ToEntityArray(Allocator.TempJob);
         
            var job = new ObstacleSeparationJob
            {
                Ships = ships,
                ShipTransformLookup = transformLookup
            };
            
            var jobModeSingleton = SystemAPI.GetSingleton<JobModeSingleton>();
            if (jobModeSingleton.JobMode == JobMode.Run)
            {
                foreach (var (localTransform, navigation, entity)
                         in SystemAPI.Query<RefRO<LocalTransform>, RefRW<Navigation>>()
                             .WithEntityAccess()
                             .WithAll<Ship>())
                {
                    float3 separationForce = float3.zero;
                    int neighborCount = 0;
            
                    foreach (Entity otherShipEntity in ships)
                    {
                        if (otherShipEntity == entity)
                            continue;

                        LocalTransform otherTransform = transformLookup[otherShipEntity];
                        float3 toOther = otherTransform.Position - localTransform.ValueRO.Position;
                        float distance = math.length(toOther);

                        const float separationDistance = 10f;
                        if (distance is < separationDistance and > 0f)
                        {
                            separationForce -= math.normalize(toOther) * (1f / distance);
                            neighborCount++;
                        }
                    }

                    if (neighborCount > 0)
                    {
                        separationForce /= neighborCount;
                        var result = math.normalize(separationForce) * 100f;
                        Debug.DrawLine(localTransform.ValueRO.Position, localTransform.ValueRO.Position + result, Color.magenta);
                        navigation.ValueRW.DesiredDirection += result; 
                    }
                }
            }
            else if (jobModeSingleton.JobMode == JobMode.Schedule)
            {
                state.Dependency = job.Schedule(state.Dependency);
                state.Dependency.Complete();
            }
            else
            {
                state.Dependency = job.ScheduleParallel(state.Dependency);
                state.Dependency.Complete();
            }
            ships.Dispose();
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
            _query.Dispose();
        }
    }
    
    [BurstCompile]
    [WithAll(typeof(Ship))]
    [WithNone(typeof(Sinking))]
    public partial struct ObstacleSeparationJob : IJobEntity
    {
        [ReadOnly] public NativeArray<Entity> Ships;
        [ReadOnly] public ComponentLookup<LocalTransform> ShipTransformLookup;

        private void Execute(in Entity entity, in LocalTransform transform, ref Navigation navigation)
        {
            float3 separationForce = float3.zero;
            int neighborCount = 0;
            
            foreach (Entity otherShipEntity in Ships)
            {
                if (otherShipEntity == entity)
                    continue;

                LocalTransform otherTransform = ShipTransformLookup[otherShipEntity];
                float3 toOther = otherTransform.Position - transform.Position;
                float distance = math.length(toOther);

                const float separationDistance = 10f;
                if (distance is < separationDistance and > 0f)
                {
                    separationForce -= math.normalize(toOther) * (1f / distance);
                    neighborCount++;
                }
            }

            if (neighborCount > 0)
            {
                separationForce /= neighborCount;
                var result = math.normalize(separationForce) * 100f;
                Debug.DrawLine(transform.Position, transform.Position + result, Color.magenta);
                navigation.DesiredDirection += result; 
            }
        }
    }
}