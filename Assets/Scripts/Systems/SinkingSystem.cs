using Components;
using Components.Enum;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Systems
{
    [BurstCompile]
    [UpdateBefore(typeof(TurnSystem))]
    [UpdateBefore(typeof(MoveSystem))] 
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial struct SinkingSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();
            state.RequireForUpdate<JobModeSingleton>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            float deltaTime = SystemAPI.Time.DeltaTime;

            var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
            var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();

            var job = new SinkingJob
            {
                DeltaTime = deltaTime,
                ECB = ecb
            };

            var jobModeSingleton = SystemAPI.GetSingleton<JobModeSingleton>();
            if (jobModeSingleton.JobMode == JobMode.Run)
            {
                job.Run();
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

        [BurstCompile]
        private partial struct SinkingJob : IJobEntity
        {
            public float DeltaTime;
            public EntityCommandBuffer.ParallelWriter ECB;

            void Execute([ChunkIndexInQuery] int sortKey,
                         ref Navigation navigation,
                         ref LocalTransform transform,
                         in Sinking sinking,
                         Entity entity)
            {
                navigation.DesiredDirection = transform.Forward();
                navigation.DesiredMoveSpeed = 0f;

                float3 up = transform.Up();
                float upDot = math.dot(up, math.up());

                // tilt til vi vender paa hovedet
                if (upDot > 0f)
                {
                    float3 tiltAxis = math.normalize(sinking.TiltAxis);
                    float tiltAngle = sinking.TiltSpeed * DeltaTime;
                    quaternion tiltRotation = quaternion.AxisAngle(tiltAxis, math.radians(tiltAngle));

                    transform.Rotation = math.normalize(
                        math.mul(transform.Rotation, tiltRotation));
                }

                float3 pos = transform.Position;
                pos.y -= sinking.SinkSpeed * DeltaTime;
                transform.Position = pos;

                // TODO: fjern hvis vi vil lave object pooling i stedet for. Det her er bare for at flocking systemet stadig virker.
                ECB.AddComponent(sortKey, entity, new DespawnBelowYLevel
                {
                    YLevel = -10f
                });
            }
        }
    }
}
