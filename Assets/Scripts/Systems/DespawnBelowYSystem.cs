using Components;
using Components.Enum;
using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;

namespace Systems
{
    //[BurstCompile]
    public partial struct DespawnBelowYSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<JobModeSingleton>();
            state.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();
        }

        //[BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
            var ecbParallel = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();

            var job = new DespawnJob
            {
                EntityCommandBuffer = ecbParallel
            };

            var jobModeSingleton = SystemAPI.GetSingleton<JobModeSingleton>();
            if (jobModeSingleton.JobMode == JobMode.Run)
            {
                state.Dependency.Complete();
                var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);
                
                foreach (var (despawnBelowYLevel, localTransform, entity)  in SystemAPI.Query<RefRW<DespawnBelowYLevel>, RefRO<LocalTransform>>().WithEntityAccess())
                {
                    if (localTransform.ValueRO.Position.y < despawnBelowYLevel.ValueRO.YLevel)
                    {
                        ecb.DestroyEntity(entity);
                    }
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

        //[BurstCompile]
        public partial struct DespawnJob : IJobEntity
        {
            public EntityCommandBuffer.ParallelWriter EntityCommandBuffer;

            void Execute(Entity entity, in DespawnBelowYLevel despawnBelowYLevel, in LocalTransform localTransform)
            {
                if (localTransform.Position.y < despawnBelowYLevel.YLevel)
                {
                    EntityCommandBuffer.DestroyEntity(entity.Index, entity);
                }
            }
        }
    }
}
