using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace Systems
{
    [BurstCompile]
    public partial struct SinkingSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            foreach (var (localToWorld, entity) in
                     SystemAPI.Query<RefRW<LocalToWorld>>()
                         .WithAll<Sinking>()
                         .WithEntityAccess())
            {
                Debug.Log($"Entity {entity} is sinking at position {localToWorld.ValueRW.Position}");
            }

        }
    }
}
