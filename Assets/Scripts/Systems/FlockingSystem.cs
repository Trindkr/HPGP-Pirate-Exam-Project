using Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Systems
{
    [BurstCompile]
    public partial struct FlockingSystem : ISystem
    {
        private EntityQuery _query;
        
        public void OnCreate(ref SystemState state)
        {
            _query = new EntityQueryBuilder(Allocator.Persistent)
                .WithAll<LocalTransform, Navigation>()
                .Build(ref state);
        }

        public void OnUpdate(ref SystemState state)
        {
            using var transforms = _query.ToComponentDataArray<LocalTransform>(Allocator.TempJob);
            var flockingJob = new FlockingJob
            {
                Transforms = transforms
            };
            flockingJob.ScheduleParallel(state.Dependency).Complete();
        }
    }

    [BurstCompile]
    public partial struct FlockingJob : IJobEntity
    {
        [ReadOnly] public NativeArray<LocalTransform> Transforms;
        private const float MaxDistance = 20f;

        public void Execute(ref LocalTransform transform, ref Navigation navigation)
        {
            float2 myPosition = transform.Position.xz;

            var separation = new float2();

            foreach (LocalTransform other in Transforms)
            {
                var otherPosition = other.Position.xz;
                var offset = otherPosition - myPosition;
                var squareDistance = math.lengthsq(offset);
                if (squareDistance > MaxDistance) continue;

                separation -= offset * (1.0f / squareDistance - 1.0f / MaxDistance);
            }

            navigation.DesiredDirection = new float3(separation.x, 0, separation.y);
        }
    }
}