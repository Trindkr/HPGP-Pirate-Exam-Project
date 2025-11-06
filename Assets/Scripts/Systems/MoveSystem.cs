using Components;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

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
    [WithNone(typeof(Sinking))] 
    public partial struct MoveJob : IJobEntity
    {
        public float DeltaTime;
        
        public void Execute(ref LocalTransform transform, ref LinearMotion motion, ref Navigation navigation)
        {
            float dot = math.dot(navigation.DesiredDirection, transform.Forward());
            navigation.DesiredMoveSpeed = math.min(navigation.DesiredMoveSpeed, motion.MaxSpeed) * math.max(dot, 0f);
        
            float desiredAcceleration = math.clamp(navigation.DesiredMoveSpeed - motion.Speed, -motion.MaxAcceleration, motion.MaxAcceleration);
            motion.Speed = math.min(motion.Speed + desiredAcceleration * DeltaTime, motion.MaxSpeed);
            transform.Position += transform.Forward() * motion.Speed * DeltaTime;
            // apply linear damping?
        }
        
    }
}