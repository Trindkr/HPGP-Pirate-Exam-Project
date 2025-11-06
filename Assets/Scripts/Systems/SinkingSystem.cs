using Unity.Burst;
using Unity.Entities;

namespace Systems
{
    [BurstCompile]
    public partial struct SinkingSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            //TODO add logic for what should happen when ship is sinking
        }
    }
}
