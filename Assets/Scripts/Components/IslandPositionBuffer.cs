using Unity.Entities;
using Unity.Mathematics;

namespace Components
{
    public struct IslandPositionBuffer : IBufferElementData
    {
        public float3 Position;
    }
}