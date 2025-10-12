using Unity.Entities;
using Unity.Mathematics;

namespace Components
{
    public struct Navigation : IComponentData
    {
        public float3 DesiredDirection;
        public float DesiredSpeed;
    }
}