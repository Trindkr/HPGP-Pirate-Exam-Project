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
    public partial struct MoveJob : IJobEntity
    {
        public float DeltaTime;
        
        public void Execute(ref LocalTransform transform, ref LinearMotion motion, ref Navigation navigation)
        {
            // Debug.DrawLine(transform.Position, transform.Position + navigation.DesiredDirection, Color.green);

            float dot = math.dot(navigation.DesiredDirection, transform.Forward());

            // problemet med at nogle skibe fik DesiredMoveSpeed = 0 var, at når de spawner,
            // kan de stå vendt væk fra DesiredDirection. Hvis skibet peger sidelæns eller
            // baglæns i forhold til desired direction, bliver dot < 0, og med math.max(dot, 0f)
            // blev hastigheden sat til 0. 
            //
            // Da skibet dermed ikke bevægede sig, kunne det heller ikke nå at dreje korrekt,
            // og resultatet var, at det blev "låst" helt stille.
            //

            //navigation.DesiredMoveSpeed = math.min(navigation.DesiredMoveSpeed, motion.MaxSpeed) * math.max(dot, 0f);

            //nyt 
            float forwardFactor = math.max(dot, 0.2f);

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