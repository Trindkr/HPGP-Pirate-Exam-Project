using Components;
using Components.Tags;
using Systems.Helpers;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;

namespace Systems
{
    [BurstCompile, UpdateBefore(typeof(TurnSystem))]
    public partial struct FlockingSystem : ISystem
    {
        private EntityQuery _query;
        private FlockingConfigurationSingleton FlockingConfigurationSingleton { get; set; }

        public void OnCreate(ref SystemState state)
        {
            _query = new EntityQueryBuilder(Allocator.Persistent)
                .WithAll<LocalTransform, Navigation>()
                .Build(ref state);
            
            state.RequireForUpdate<FlockingConfigurationSingleton>();
        }

        public void OnUpdate(ref SystemState state)
        {
            FlockingConfigurationSingleton = SystemAPI.GetSingleton<FlockingConfigurationSingleton>();
            using var transforms = _query.ToComponentDataArray<LocalTransform>(Allocator.TempJob);
            var flockingJob = new FlockingJob
            {
                Transforms = transforms,
                FlockingConfigurationSingleton = FlockingConfigurationSingleton,
            };
            flockingJob.ScheduleParallel(state.Dependency).Complete();
        }
    }
    
    [WithAll(typeof(AllFlockingTag))]
    [BurstCompile]
    public partial struct FlockingJob : IJobEntity
    {
        [ReadOnly] public NativeArray<LocalTransform> Transforms;
        public FlockingConfigurationSingleton FlockingConfigurationSingleton;
        
        // make configurable
        private const float MaxDistance = 500f;

        private void Execute(in LocalTransform transform, ref Navigation navigation)
        {
            Flocker.Flock(ref navigation, transform, Transforms, FlockingConfigurationSingleton.FlockingConfiguration, MaxDistance);
        }
    }
}