using Components.Enum;
using Unity.Entities;

namespace Components.Cannon
{
    public struct CannonConstraints : IComponentData
    {
        public float ShootingForce;
        public float ShootingAngle;
        public float ShootingRange;
        public float ReloadTime;
        public float ReloadTimer;
        public ShootingDirection ShootingDirection;
    }
}
