using Components;
using Components.Enum;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;

[BurstCompile]
[UpdateInGroup(typeof(SimulationSystemGroup))]
public partial struct DespawnBelowYSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<JobModeSingleton>();
        state.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();

        var job = new DespawnJob
        {
            EntityCommandBuffer = ecb
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
    public partial struct DespawnJob : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter EntityCommandBuffer;

        void Execute(Entity entity, ref DespawnBelowYLevel despawnBelowYLevel, ref LocalTransform localTransform)
        {
            if (localTransform.Position.y < despawnBelowYLevel.YLevel)
            {
                EntityCommandBuffer.DestroyEntity(entity.Index, entity);
            }
        }
    }
}
