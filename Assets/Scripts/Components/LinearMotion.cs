using Unity.Entities;

namespace Components
{
    public struct LinearMotion : IComponentData
    {
        public float Speed;
        public float MaxSpeed;
        public float MaxAcceleration;
    }
}