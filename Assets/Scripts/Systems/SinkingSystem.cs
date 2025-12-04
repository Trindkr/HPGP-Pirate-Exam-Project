using Components;
using Components.Enum;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics.Systems;
using Unity.Transforms;

namespace Systems
{
    [BurstCompile]
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
            var ecbParallel = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();

            var job = new SinkingJob
            {
                DeltaTime = deltaTime,
                ECB = ecbParallel
            };

            var jobModeSingleton = SystemAPI.GetSingleton<JobModeSingleton>();
            
            if (jobModeSingleton.JobMode == JobMode.Run)
            {
                state.Dependency.Complete();
                var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);
                foreach (var (navigation, localTransform, sinking, entity) in SystemAPI.Query<RefRW<Navigation>, RefRW<LocalTransform>, RefRO<Sinking>>().WithEntityAccess())
                {
                    navigation.ValueRW.DesiredDirection = localTransform.ValueRO.Forward();
                    navigation.ValueRW.DesiredMoveSpeed = 0f;

                    float3 up = localTransform.ValueRO.Up();
                    float upDot = math.dot(up, math.up());

                    if (upDot > 0f)
                    {
                        float3 tiltAxis = math.normalize(sinking.ValueRO.TiltAxis);
                        float tiltAngle = sinking.ValueRO.TiltSpeed * deltaTime;
                        quaternion tiltRotation = quaternion.AxisAngle(tiltAxis, math.radians(tiltAngle));

                        localTransform.ValueRW.Rotation = math.normalize(
                            math.mul(localTransform.ValueRO.Rotation, tiltRotation));
                    }

                    float3 pos = localTransform.ValueRO.Position;
                    pos.y -= sinking.ValueRO.SinkSpeed * deltaTime;
                    localTransform.ValueRW.Position = pos;

                    ecb.AddComponent(entity, new DespawnBelowYLevel
                    {
                        YLevel = -10f
                    });
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

                ECB.AddComponent(sortKey, entity, new DespawnBelowYLevel
                {
                    YLevel = -10f
                });
            }
        }
    }
}
