using Components;
using Components.Enum;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Systems
{
    //[BurstCompile]
    [UpdateAfter(typeof(TurnSystem))]
    public partial struct MoveSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<JobModeSingleton>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var deltaTime = SystemAPI.Time.DeltaTime;
            var job = new MoveJob
            {
                DeltaTime = deltaTime
            };
            
            var jobModeSingleton = SystemAPI.GetSingleton<JobModeSingleton>();
            if (jobModeSingleton.JobMode == JobMode.Run)
            {
                foreach (var (transform, motion, navigation) 
                         in SystemAPI.Query<RefRW<LocalTransform>, RefRW<LinearMotion>, RefRW<Navigation>>())
                {
                    float dot = math.dot(navigation.ValueRO.DesiredDirection, transform.ValueRO.Forward());
                    var normalizedDot = (dot + 1) * 0.5f;
                    float forwardFactor = math.max(normalizedDot, 0.1f);

                    navigation.ValueRW.DesiredMoveSpeed =
                        math.min(navigation.ValueRO.DesiredMoveSpeed, motion.ValueRO.MaxSpeed)
                        * forwardFactor;

                    float desiredAcceleration = math.clamp(navigation.ValueRO.DesiredMoveSpeed - motion.ValueRO.Speed, -motion.ValueRO.MaxAcceleration, motion.ValueRO.MaxAcceleration);
                    motion.ValueRW.Speed = math.min(motion.ValueRO.Speed + desiredAcceleration * deltaTime, motion.ValueRO.MaxSpeed);
                    transform.ValueRW.Position += transform.ValueRO.Forward() * motion.ValueRO.Speed * deltaTime;
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

    //[BurstCompile]
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