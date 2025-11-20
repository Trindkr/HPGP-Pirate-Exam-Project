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

                var pirateShipAmount = authoring._simulationConfig.ShipAmount / 2;
                var merchantShipAmount = authoring._simulationConfig.ShipAmount / 2;
                
                
                AddComponent(spawner, new FleetSpawner
                {
                    PirateShipAmount = pirateShipAmount,
                    MerchantShipAmount = merchantShipAmount,
                    MerchantShipsPerFleet = authoring._simulationConfig.FleetSize,
                    PirateShipsPerFleet = authoring._simulationConfig.FleetSize,
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