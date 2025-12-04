using Components;
using Components.Enum;
using Components.Fleet;
using Model;
using Systems.Helpers;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;

namespace Systems.Fleet
{
    [BurstCompile]
    public partial struct FleetFlockingSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<JobModeSingleton>();
            state.RequireForUpdate<FlockingConfigurationSingleton>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var fleetShipBufferLookup = SystemAPI.GetBufferLookup<FleetShipBuffer>(true);
            var localTransformLookup = SystemAPI.GetComponentLookup<LocalTransform>(true);
            
            var flockingConfiguration = SystemAPI.GetSingleton<FlockingConfigurationSingleton>().FlockingConfiguration;
            FleetFlockingJob job = new FleetFlockingJob
            {
                FleetShipBufferLookup = fleetShipBufferLookup,
                LocalTransformLookup = localTransformLookup,
                FlockingConfiguration = flockingConfiguration,
            };
            
            var jobModeSingleton = SystemAPI.GetSingleton<JobModeSingleton>();
            if (jobModeSingleton.JobMode == JobMode.Run)
            {
                state.Dependency.Complete();
                foreach (var (navigation, fleetMember, entity) in SystemAPI
                             .Query<RefRW<Navigation>, RefRO<FleetMember>>().WithEntityAccess())
                {
                    if (!fleetShipBufferLookup.HasBuffer(fleetMember.ValueRO.FleetEntity))
                        return;

                    var fleetBuffer = fleetShipBufferLookup[fleetMember.ValueRO.FleetEntity];
                    var ships = fleetBuffer.AsNativeArray();

                    // Collect transforms of all ships in this fleet
                    var fleetTransforms = new NativeArray<LocalTransform>(ships.Length, Allocator.Temp);
                    for (int i = 0; i < ships.Length; i++)
                    {
                        var shipEntity = ships[i].ShipEntity;
                        if (localTransformLookup.HasComponent(shipEntity))
                            fleetTransforms[i] = localTransformLookup[shipEntity];
                    }

                    Flocker.Flock(
                        ref navigation.ValueRW, 
                        localTransformLookup[entity], 
                        fleetTransforms, 
                        flockingConfiguration);

                    fleetTransforms.Dispose();
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
   }

    [BurstCompile]
    [WithNone(typeof(Sinking))]
    //TODO: m�ske skal sinking skibe ogs� fjernes fra flocking, s� skibe ikke flocker med sunkne skibe?
    // PT. flocker de levende skibe stadig med de sunkne skibe, selvom sunkne skibe ikke flocker (har ingen desired speed/direction)
    //Andre m�der at h�ndtere det p�? Vi snakkede om object pooling.
    public partial struct FleetFlockingJob : IJobEntity
    {
        [ReadOnly] public BufferLookup<FleetShipBuffer> FleetShipBufferLookup;
        [ReadOnly] public ComponentLookup<LocalTransform> LocalTransformLookup;
        public FlockingConfiguration FlockingConfiguration;
        
        private void Execute(Entity entity, ref Navigation navigation, in FleetMember fleetMember)
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
}