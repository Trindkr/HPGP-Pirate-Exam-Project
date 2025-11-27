using Unity.Entities;
using Components.Enum;

namespace Components
{
    public struct JobModeSingleton : IComponentData
    {
        public JobMode JobMode;
    }
}