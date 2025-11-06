using Components.Fleet;
using Model;
using ScriptableObjects;
using Unity.Entities;
using UnityEngine;

namespace Authoring
{
    public class FleetSpawnerAuthoring : MonoBehaviour
    {
        [SerializeField] private SimulationConfig _simulationConfig;
        [SerializeField] private GameObject _pirateShipPrefab;
        [SerializeField] private GameObject _merchantShipPrefab;
        [SerializeField] private GameObject _cannonballPrefab;
        
        
        private class FleetSpawnerBaker : Baker<FleetSpawnerAuthoring>
        {
            public override void Bake(FleetSpawnerAuthoring authoring)
            {
                var spawner = GetEntity(authoring, TransformUsageFlags.None);
                
                AddComponent(spawner, new FleetSpawner
                {
                    
                    NumberOfPirateFleets = authoring._simulationConfig.NumberOfPirateFleets,
                    NumberOfMerchantFleets = authoring._simulationConfig.NumberOfMerchantFleets,
                    MerchantShipsPerFleet = 
                        authoring._simulationConfig.NumberOfMerchantShips 
                        / authoring._simulationConfig.NumberOfMerchantFleets,
                    PirateShipsPerFleet = 
                        authoring._simulationConfig.NumberOfPirateShips 
                        / authoring._simulationConfig.NumberOfPirateFleets,
                    PirateShipPrefab = GetEntity(authoring._pirateShipPrefab, TransformUsageFlags.Dynamic),
                    MerchantShipPrefab = GetEntity(authoring._merchantShipPrefab, TransformUsageFlags.Dynamic),
                    SailingConstraints = authoring._simulationConfig.SailingConstraints,
                    CannonConfiguration = authoring._simulationConfig.CannonConfiguration,
                    CannonballPrefab = GetEntity(authoring._cannonballPrefab, TransformUsageFlags.Dynamic)
                });
            }
        }
    }
}