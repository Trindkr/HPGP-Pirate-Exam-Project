using Model;
using Unity.Entities;

namespace Components
{
    public struct TradeShipSpawner : IComponentData
    {
        public Entity ShipPrefab;
        public int NumberOfShips;
        public SailingConstraints SailingConstraints;
    }
}