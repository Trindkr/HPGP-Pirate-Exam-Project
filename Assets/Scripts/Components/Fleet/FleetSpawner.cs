using Unity.Entities;

namespace Components.Fleet
{
    public struct FleetSpawner : IComponentData
    {
        public int NumberOfPirateFleets;
        public int NumberOfMerchantFleets;
        public int MaxPirateFleetSize;
        public int MaxMerchantFleetSize;
    }
}