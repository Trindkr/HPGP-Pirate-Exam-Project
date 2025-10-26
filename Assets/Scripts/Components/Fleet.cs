using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;

namespace Components
{
    public struct Fleet : IComponentData
    {
        public NativeList<LocalTransform> FleetShips;
        public int MaxFleetSize;
    }
}