using Model;
using Unity.Entities;

namespace Components
{
    public struct PirateShipSpawner : IComponentData
    {
        public Entity ShipPrefab;
        public Entity CannonballPrefab;
        public int NumberOfShips;
        public SailingConstraints  SailingConstraints;
        public Model.CannonConstraints CannonConstraints;
    }
}