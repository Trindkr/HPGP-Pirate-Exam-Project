using System;

namespace Model
{
    [Serializable]
    public struct CannonConfiguration
    {
        public float ReloadTime;
        public float MaxShootingForce;
        public float MinShootingForce;
        public float MaxShootingAngle;
        public float MinShootingAngle;
        
    }
}