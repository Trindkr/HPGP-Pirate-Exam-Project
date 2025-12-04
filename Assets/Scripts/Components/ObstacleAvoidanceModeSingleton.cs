using Components.Enum;
using Unity.Entities;

namespace Components
{
    public struct ObstacleAvoidanceModeSingleton : IComponentData
    {
        public ObstacleAvoidanceMode ObstacleAvoidanceMode;
    }
}