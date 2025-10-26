using Components.Fleet;
using ScriptableObjects;
using Unity.Entities;
using UnityEngine;

namespace Authoring
{
    public class FleetSpawnerAuthoring : MonoBehaviour
    {
        [SerializeField] private SimulationConfig _simulationConfig;

        private class FleetSpawnerBaker : Baker<FleetSpawnerAuthoring>
        {
            public override void Bake(FleetSpawnerAuthoring authoring)
            {
                var spawner = GetEntity(authoring, TransformUsageFlags.None);
                
                AddComponent(spawner, new FleetSpawner
                {
                    NumberOfPirateFleets = authoring._simulationConfig.NumberOfPirateFleets,
                    NumberOfMerchantFleets = authoring._simulationConfig.NumberOfMerchantFleets,
                    MaxMerchantFleetSize = 
                        authoring._simulationConfig.NumberOfTradeShips 
                        / authoring._simulationConfig.NumberOfMerchantFleets,
                    MaxPirateFleetSize = 
                        authoring._simulationConfig.NumberOfPirateFleets 
                        / authoring._simulationConfig.NumberOfPirateFleets,
                });
            }
        }
    }
}