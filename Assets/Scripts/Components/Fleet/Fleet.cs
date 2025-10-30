using Unity.Entities;
using Unity.Mathematics;

namespace Components.Fleet
{
    public struct Fleet : IComponentData
    {
        public float2 Center;
        public float2 Alignment;
    }
}