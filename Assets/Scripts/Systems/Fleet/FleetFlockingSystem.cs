using Components;
using Components.Fleet;
using Systems.Helpers;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;

namespace Systems.Fleet
{
    [BurstCompile, UpdateBefore(typeof(TurnSystem))]
    public partial struct FleetFlockingSystem : ISystem
    {
        private FlockingConfigurationSingleton FlockingConfigurationSingleton { get; set; }

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<FlockingConfigurationSingleton>();
        }

        public void OnUpdate(ref SystemState state)
        {
            FlockingConfigurationSingleton = SystemAPI.GetSingleton<FlockingConfigurationSingleton>();
            FleetFlockingJob job = new FleetFlockingJob
            {
                FleetShipBufferLookup = SystemAPI.GetBufferLookup<FleetShipBuffer>(true),
                LocalTransformLookup = SystemAPI.GetComponentLookup<LocalTransform>(true),
                FlockingConfigurationSingleton = FlockingConfigurationSingleton,
            };
            state.Dependency = job.ScheduleParallel(state.Dependency);
        }
   }

    [BurstCompile]
    public partial struct FleetFlockingJob : IJobEntity
    {
        [ReadOnly] public BufferLookup<FleetShipBuffer> FleetShipBufferLookup;
        [ReadOnly] public ComponentLookup<LocalTransform> LocalTransformLookup;
        public FlockingConfigurationSingleton FlockingConfigurationSingleton;
        
        private void Execute(Entity entity, ref Navigation navigation, FleetMember fleetMember)
        {
            if (!FleetShipBufferLookup.HasBuffer(fleetMember.FleetEntity))
                return;

            var fleetBuffer = FleetShipBufferLookup[fleetMember.FleetEntity];
            var ships = fleetBuffer.AsNativeArray();

            // Collect transforms of all ships in this fleet
            var fleetTransforms = new NativeArray<LocalTransform>(ships.Length, Allocator.Temp);
            for (int i = 0; i < ships.Length; i++)
            {
                var shipEntity = ships[i].ShipEntity;
                if (LocalTransformLookup.HasComponent(shipEntity))
                    fleetTransforms[i] = LocalTransformLookup[shipEntity];
            }

            Flocker.Flock(
                ref navigation, 
                LocalTransformLookup[entity], 
                fleetTransforms, 
                FlockingConfigurationSingleton.FlockingConfiguration);

            fleetTransforms.Dispose();
        }
    }
}