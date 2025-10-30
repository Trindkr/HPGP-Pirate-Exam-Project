using Model;
using Unity.Entities;

namespace Components.Fleet
{
    public struct FleetSpawner : IComponentData
    {
        public int NumberOfPirateFleets;
        public int NumberOfMerchantFleets;
        public int PirateShipsPerFleet;
        public int MerchantShipsPerFleet;
        
        public Entity PirateShipPrefab;
        public Entity MerchantShipPrefab;
        public SailingConstraints SailingConstraints;
        
        public Entity CannonballPrefab;
        public Model.CannonConstraints CannonConstraints;
    }
}