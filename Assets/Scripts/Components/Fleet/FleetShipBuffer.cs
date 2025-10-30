using Unity.Entities;

namespace Components.Fleet
{
    public struct FleetShipBuffer : IBufferElementData
    {
        public Entity ShipEntity;
    }
}