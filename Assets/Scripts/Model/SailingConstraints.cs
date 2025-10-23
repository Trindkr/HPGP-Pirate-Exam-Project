using System;

namespace Model
{
    [Serializable]
    public struct SailingConstraints
    {
        public float MaxAngularAcceleration;
        public float MaxAngularSpeed;
        public float MaxLinearAcceleration;
        public float MaxLinearSpeed;
    }
}