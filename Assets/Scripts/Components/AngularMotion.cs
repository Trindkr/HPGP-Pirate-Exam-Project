using Unity.Entities;

namespace Components
{
    public struct AngularMotion  : IComponentData
    {
        public float Speed;
        public float MaxSpeed;
        public float Acceleration;
        public float MaxAcceleration;
    }
}