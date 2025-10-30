using Components;
using ScriptableObjects;
using Unity.Entities;
using UnityEngine;

namespace Authoring
{
    public class PirateShipSpawnerAuthoring : MonoBehaviour
    {
        public GameObject ShipPrefab;
        public GameObject CannonballPrefab;
        public SimulationConfig SimulationConfig;
        
        private class PirateShipSpawnerBaker : Baker<PirateShipSpawnerAuthoring>
        {
            public override void Bake(PirateShipSpawnerAuthoring authoring)
            {
                var spawner = GetEntity(authoring, TransformUsageFlags.None);
                
                AddComponent(spawner, new PirateShipSpawner
                {
                    ShipPrefab = GetEntity(authoring.ShipPrefab, TransformUsageFlags.Dynamic),
                    CannonballPrefab = GetEntity(authoring.CannonballPrefab, TransformUsageFlags.Dynamic),
                    NumberOfShips = authoring.SimulationConfig.NumberOfPirateShips,
                    SailingConstraints = authoring.SimulationConfig.SailingConstraints,
                    CannonConstraintsConfig = authoring.SimulationConfig.cannonConstraintsConfig
                });
            }
        }
    }
}