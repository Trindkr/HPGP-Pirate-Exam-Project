using Unity.Entities;

namespace Components
{
    public struct ShipSpawner : IComponentData
    {
        public Entity ShipPrefab;
        public int NumberOfShips;
    }
}