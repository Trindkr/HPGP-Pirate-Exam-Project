using Model;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;

namespace Components.Fleet
{
    public struct Fleet : IComponentData
    {
        public NativeArray<LocalTransform> FleetShips;
        public int MaxFleetSize;
        public ShipType FleetType;

        public Fleet(int maxFleetSize, ShipType fleetType)
        {
            FleetType = fleetType;
            MaxFleetSize = maxFleetSize;
            FleetShips = new NativeArray<LocalTransform>(MaxFleetSize, Allocator.Persistent);
        }
    }
}