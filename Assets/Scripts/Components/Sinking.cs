using Unity.Mathematics;
using Unity.Entities;

namespace Components
{
    public struct Sinking : IComponentData
    {
        public float SinkSpeed;      
        public float TiltSpeed;       
        public float3 TiltAxis;       
    }
}
