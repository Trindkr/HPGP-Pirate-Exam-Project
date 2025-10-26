using Components.Fleet;
using Model;
using Unity.Entities;

namespace Systems
{
    public partial struct FleetSpawnerSystem : ISystem, ISystemStartStop
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>();
        }
        
        public void OnStartRunning(ref SystemState state)
        {
            var ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
            var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);
            foreach (var spawner in SystemAPI.Query<RefRO<FleetSpawner>>())
            {
                for (int i = 0; i < spawner.ValueRO.NumberOfPirateFleets; i++)
                {
                    SpawnFleet(ShipType.Pirate, spawner.ValueRO.MaxPirateFleetSize, ref ecb);
                }
                
                for (int i = 0; i < spawner.ValueRO.NumberOfMerchantFleets; i++)
                {
                    SpawnFleet(ShipType.Merchant, spawner.ValueRO.MaxMerchantFleetSize, ref ecb);
                }
            }
        }

        private void SpawnFleet(ShipType fleetType, int maxFleetSize, ref EntityCommandBuffer ecb)
        {
            var fleet = ecb.CreateEntity();
            ecb.AddComponent(fleet, new Fleet(maxFleetSize, fleetType));
        }

        public void OnStopRunning(ref SystemState state)
        {
        }
    }
}