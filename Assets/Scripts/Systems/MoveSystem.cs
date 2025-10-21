using System.Runtime.CompilerServices;
using Components;
using ExtensionMethods;
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
            moveJob.ScheduleParallel(state.Dependency).Complete();
        }
    }

    [BurstCompile]
    public partial struct MoveJob : IJobEntity
    {
        public float DeltaTime;

        public void Execute(ref LocalTransform transform, ref LinearMotion motion, in Navigation navigation)
        {
            float desiredAcceleration = navigation.DesiredMoveSpeed - motion.Speed;
            float clampedAcceleration =
                math.clamp(desiredAcceleration, -motion.MaxAcceleration, motion.MaxAcceleration);
            motion.Speed += clampedAcceleration;
            motion.Speed = math.min(motion.Speed, motion.MaxSpeed);
            transform.MoveForward(motion.Speed * DeltaTime);
        }
    }
}