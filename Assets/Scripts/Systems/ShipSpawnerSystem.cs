using Components;
using Unity.Entities;

namespace Systems
{
    public partial struct ShipSpawnerSystem : ISystem, ISystemStartStop
    {
        public void OnStartRunning(ref SystemState state)
        {
            SpawnBoats(ref state);
        }

        public void OnStopRunning(ref SystemState state)
        {
        }

        private void SpawnBoats(ref SystemState state)
        {
            foreach (var spawner in SystemAPI.Query<RefRO<ShipSpawner>>())
            {
                for (int i = 0; i < spawner.ValueRO.NumberOfShips; i++)
                {
                    state.EntityManager.Instantiate(spawner.ValueRO.ShipPrefab);
                }
            }
        }
    }
}