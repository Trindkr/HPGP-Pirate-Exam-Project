using Components;
using Unity.Entities;
using Unity.Transforms;

namespace Systems
{
    public partial struct SailingSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            var sailingJob = new SailingJob
            {
                DeltaTime = SystemAPI.Time.DeltaTime
            };

            sailingJob.ScheduleParallel();
        }
    }

    public partial struct SailingJob : IJobEntity
    {
        public float DeltaTime;
        private const float Speed = 1f;

        public void Execute(ref LocalTransform transform, in Ship ship)
        {
            transform.Position += DeltaTime * Speed * transform.Forward();
        }
    }
}