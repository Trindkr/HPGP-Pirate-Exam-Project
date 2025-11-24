using Components;
using Systems.Fleet;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Systems
{
    [BurstCompile]
    [UpdateBefore(typeof(TurnSystem)), UpdateAfter(typeof(FleetFlockingSystem))]
    public partial struct ObstacleSeparationSystem : ISystem
    {
        private NativeArray<Entity> _ships;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            _ships =
                SystemAPI.QueryBuilder()
                    .WithAll<Ship>()
                    .WithNone<Sinking>()
                    .WithAll<LocalTransform>()
                    .Build().ToEntityArray(Allocator.Persistent);

        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var transformLookup = SystemAPI.GetComponentLookup<LocalTransform>(true);
            var job = new ObstacleSeparationJob
            {
                Ships = _ships,
                ShipTransformLookup = transformLookup
            };
            state.Dependency = job.ScheduleParallel(state.Dependency);
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
            if (_ships.IsCreated)
            {
                _ships.Dispose();
            }
        }
    }
    
    [BurstCompile]
    public partial struct ObstacleSeparationJob : IJobEntity
    {
        [ReadOnly] public NativeArray<Entity> Ships;
        [ReadOnly] public ComponentLookup<LocalTransform> ShipTransformLookup;
        
        public void Execute(in Entity entity, in LocalTransform transform, in Ship ship, ref Navigation navigation)
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

                float separationDistance = 5f; // Minimum desired separation distance
                if (distance < separationDistance && distance > 0f)
                {
                    separationForce -= math.normalize(toOther) / distance;
                    neighborCount++;
                }
            }
            

            if (neighborCount > 0)
            {
                separationForce /= neighborCount;
                navigation.DesiredDirection += math.normalize(separationForce) * 10f; // Weight the force
            }
        }
    }
}