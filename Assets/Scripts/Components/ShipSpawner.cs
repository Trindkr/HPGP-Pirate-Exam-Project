using Unity.Entities;

namespace Components
{
    public struct ShipSpawner : IComponentData
    {
        public Entity ShipPrefab;
        public int NumberOfShips;
        public float MaxAngularAcceleration;
        public float MaxAngularSpeed;
        public float MaxLinearAcceleration;
        public float MaxLinearSpeed;
    }
}