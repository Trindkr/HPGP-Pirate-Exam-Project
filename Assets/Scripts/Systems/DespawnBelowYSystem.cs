using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;

[BurstCompile]
[UpdateInGroup(typeof(SimulationSystemGroup))]
public partial struct DespawnBelowYSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();

        var job = new DespawnJob
        {
            EntityCommandBuffer = ecb
        };

        job.ScheduleParallel();
    }

    [BurstCompile]
    public partial struct DespawnJob : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter EntityCommandBuffer;

        void Execute(Entity entity, ref DespawnBelowYLevel despawnBelowYLevel, ref LocalTransform localTransform)
        {
            if (localTransform.Position.y < despawnBelowYLevel.YLevel)
            {
                //EntityCommandBuffer.DestroyEntity(entity.Index, entity);
            }
        }
    }
}
