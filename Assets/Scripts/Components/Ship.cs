using Unity.Entities;
using Unity.Mathematics;

namespace Components
{
    public struct Ship : IComponentData
    {
        public Random Random;
        public float MaxTurningSpeed;
        public float Speed;
        public float Angle;
        public float AngularVelocity;
        public float TurnTimer;
    }
}