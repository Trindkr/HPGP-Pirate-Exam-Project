using Unity.Entities;

namespace Components
{
    public struct MerchantSeeker : IComponentData
    {
        public Entity Target;
    }
}