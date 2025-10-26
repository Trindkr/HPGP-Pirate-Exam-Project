using Components;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine.SocialPlatforms;

namespace Systems
{
    public partial struct FleetFlockingSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            FleetFlockingJob job = new FleetFlockingJob();
            state.Dependency = job.ScheduleParallel(state.Dependency);
        }
   }

    public partial struct FleetFlockingJob : IJobEntity
    {
        private void Execute(in LocalTransform localTransform, in FleetMember fleetMember)
        {
            
        }
    }
}