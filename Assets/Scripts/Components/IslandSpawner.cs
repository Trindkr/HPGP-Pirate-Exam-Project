using Unity.Entities;

namespace Components
{
    public struct IslandSpawner : IComponentData
    {
        public Entity IslandPrefab;
        public int IslandAmount;
        public float Radius;
    }
}