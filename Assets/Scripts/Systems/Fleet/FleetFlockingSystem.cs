using Components;
using Components.Fleet;
using ExtensionMethods;
using Model;
using Systems.Helpers;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Systems.Fleet
{
    [BurstCompile, UpdateBefore(typeof(TurnSystem))]
    public partial struct FleetFlockingSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<FlockingConfigurationSingleton>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var flockingConfiguration = SystemAPI.GetSingleton<FlockingConfigurationSingleton>().FlockingConfiguration;
            var fleetFlockingJob = new FleetFlockingJob2
            {
                FleetShipBufferLookup = SystemAPI.GetBufferLookup<FleetShipBuffer>(true),
                LocalTransformLookup = SystemAPI.GetComponentLookup<LocalTransform>(true),
                FlockingConfiguration = flockingConfiguration,
            };
            state.Dependency = fleetFlockingJob.ScheduleParallel(state.Dependency);

            var flockingNavigationJob = new FlockingNavigationJob
            {
                FleetLookup = SystemAPI.GetComponentLookup<Components.Fleet.Fleet>(true)
            };
            state.Dependency = flockingNavigationJob.ScheduleParallel(state.Dependency);
        }
   }

    [BurstCompile]
    public partial struct FleetFlockingJob : IJobEntity
    {
        [ReadOnly] public BufferLookup<FleetShipBuffer> FleetShipBufferLookup;
        [ReadOnly] public ComponentLookup<LocalTransform> LocalTransformLookup;
        public FlockingConfiguration FlockingConfiguration;
        
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
                FlockingConfiguration);

            fleetTransforms.Dispose();
        }
    }

    public partial struct FleetFlockingJob2 : IJobEntity
    {
        [ReadOnly] public BufferLookup<FleetShipBuffer> FleetShipBufferLookup;
        [ReadOnly] public ComponentLookup<LocalTransform> LocalTransformLookup;
        public FlockingConfiguration FlockingConfiguration;

        private void Execute(Entity entity, ref Components.Fleet.Fleet fleet)
        {
            if (!FleetShipBufferLookup.HasBuffer(entity))
                return;
            
            var fleetBuffer = FleetShipBufferLookup[entity];
            var ships = fleetBuffer.AsNativeArray();
            
            // Collect transforms of all ships in this fleet
            var fleetTransforms = new NativeArray<LocalTransform>(ships.Length, Allocator.Temp);
            for (int i = 0; i < ships.Length; i++)
            {
                var shipEntity = ships[i].ShipEntity;
                if (LocalTransformLookup.HasComponent(shipEntity))
                    fleetTransforms[i] = LocalTransformLookup[shipEntity];
            }
            
            Flocker.Flock(ref fleet, fleetTransforms, FlockingConfiguration);
            
            fleetTransforms.Dispose();
        }
    }
    
    public partial struct FlockingNavigationJob : IJobEntity
    {
        [ReadOnly] public ComponentLookup<Components.Fleet.Fleet> FleetLookup;

        private void Execute(ref Navigation navigation, FleetMember fleetMember)
        {
            if (FleetLookup.HasComponent(fleetMember.FleetEntity))
            {
                var fleet = FleetLookup[fleetMember.FleetEntity];

                var direction = fleet.Center + fleet.Alignment;
                navigation.DesiredDirection = direction.x0z();
                
                var magnitudeSquared = math.lengthsq(navigation.DesiredDirection);
                navigation.DesiredMoveSpeed = magnitudeSquared;
            }
        }
    }
}