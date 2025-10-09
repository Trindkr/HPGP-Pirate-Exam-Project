using Components;
using Unity.Entities;
using Unity.Mathematics;
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

            var jobHandle = sailingJob.ScheduleParallel(state.Dependency);
            state.Dependency = jobHandle;
        }
    }
    
    public partial struct SailingJob : IJobEntity
    {
        public float DeltaTime;

        private void Execute(ref LocalTransform transform, ref Ship ship)
        {
            ship.TurnTimer -= DeltaTime;
            if (ship.TurnTimer <= 0f)
            {
                ship.AngularVelocity = ship.Random.NextFloat(-ship.MaxTurningSpeed, ship.MaxTurningSpeed);
                ship.TurnTimer = ship.Random.NextFloat(1.0f, 3.0f);
            }
            
            ship.Angle += ship.AngularVelocity * DeltaTime;
            transform.Rotation = quaternion.RotateY(ship.Angle);
            var velocity = ship.Speed * transform.Forward();
            transform.Position += DeltaTime * velocity;
        }
    }
}