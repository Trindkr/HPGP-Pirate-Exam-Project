using Components;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Systems
{
    [BurstCompile]
    public partial struct MoveSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            var moveJob = new MoveJob
            {
                DeltaTime = SystemAPI.Time.DeltaTime
            };
            state.Dependency = moveJob.ScheduleParallel(state.Dependency);
        }
    }

    [BurstCompile]
    public partial struct MoveJob : IJobEntity
    {
        public float DeltaTime;
        
        public void Execute(ref LocalTransform transform, ref LinearMotion motion, ref Navigation navigation)
        {
            float dot = math.dot(navigation.DesiredDirection, transform.Forward());
            var normalizedDot = (dot + 1) * 0.5f;
            float forwardFactor = math.max(normalizedDot, 0.1f);

            navigation.DesiredMoveSpeed =
                math.min(navigation.DesiredMoveSpeed, motion.MaxSpeed)
                * forwardFactor;

            float desiredAcceleration = math.clamp(navigation.DesiredMoveSpeed - motion.Speed, -motion.MaxAcceleration, motion.MaxAcceleration);
            motion.Speed = math.min(motion.Speed + desiredAcceleration * DeltaTime, motion.MaxSpeed);
            transform.Position += transform.Forward() * motion.Speed * DeltaTime;
            // apply linear damping?
        }
        
    }
}