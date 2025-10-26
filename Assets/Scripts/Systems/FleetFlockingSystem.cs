using Components;
using Components.Fleet;
using Systems.Helpers;
using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;

namespace Systems
{
    [BurstCompile]
    public partial struct FleetFlockingSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            FleetFlockingJob job = new FleetFlockingJob();
            state.Dependency = job.ScheduleParallel(state.Dependency);
        }
   }
    
    [BurstCompile]
    public partial struct FleetFlockingJob : IJobEntity
    {
        private void Execute(ref Navigation navigation, in LocalTransform localTransform, in FleetMember fleetMember)
        {
            Flocker.Flock(ref navigation, localTransform, fleetMember.Fleet.FleetShips);
        }
    }
}