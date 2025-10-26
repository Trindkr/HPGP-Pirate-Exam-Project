using Unity.Entities;

namespace Components
{
    public struct FleetMember : IComponentData
    {
        public Fleet Fleet;
    }
}