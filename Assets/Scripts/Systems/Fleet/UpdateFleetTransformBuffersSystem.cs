using Unity.Entities;

namespace Systems.Fleet
{
    public partial struct UpdateFleetTransformBuffersSystem : ISystem
    {
        // public void OnUpdate(ref SystemState state)
        // {
        //     var allocator = state.WorldUpdateAllocator;
        //     
        //     var shipQuery = SystemAPI.QueryBuilder()
        //         .WithAll<FleetMember, LocalTransform>()
        //         .Build();
        //
        //     var fleetMap = new NativeParallelMultiHashMap<Entity, Entity>(
        //         shipQuery.CalculateEntityCount(), allocator);
        //     
        //     foreach (var (member, transform, entity) in 
        //              SystemAPI.Query<RefRO<FleetMember>, RefRO<LocalTransform>>().WithEntityAccess())
        //     {
        //         fleetMap.Add(member.ValueRO.FleetEntity, entity);
        //     }
        //
        //     foreach (var (_, entity) in SystemAPI.Query<RefRO<Fleet>>().WithEntityAccess())
        //     {
        //         var buffer = state.EntityManager.GetBuffer<FleetShipBuffer>(entity);
        //         buffer.Clear();
        //         
        //         foreach (var localTransform in fleetMap.GetValuesForKey(entity))
        //         {
        //             buffer.Add(new FleetShipBuffer {  });
        //         }
        //     }
        // }
    }
}