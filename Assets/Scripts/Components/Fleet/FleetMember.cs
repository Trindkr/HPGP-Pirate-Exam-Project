using Model;
using Unity.Entities;

namespace Components.Fleet
{
    public struct FleetMember : IComponentData
    {
        public Fleet Fleet;
        public ShipType ShipType;
    }
}