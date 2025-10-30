using Model;
using Unity.Entities;

namespace Components
{
    public struct FlockingConfigurationSingleton : IComponentData
    {
        public FlockingConfiguration FlockingConfiguration;
    }
}