using Unity.Entities;

namespace Components.Fleet
{
    public struct FleetMember : IComponentData
    {
        public Entity FleetEntity;
    }
}