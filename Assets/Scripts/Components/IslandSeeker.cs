using Unity.Entities;
using Unity.Mathematics;

namespace Components
{
    public struct IslandSeeker : IComponentData
    {
        public int IslandIndex;
    }
}