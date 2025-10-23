using Components;
using ScriptableObjects;
using Unity.Entities;
using UnityEngine;

namespace Authoring
{
    public class PirateShipSpawnerAuthoring : MonoBehaviour
    {
        public GameObject Prefab;
        public SimulationConfig SimulationConfig;
        
        private class PirateShipSpawnerBaker : Baker<PirateShipSpawnerAuthoring>
        {
            public override void Bake(PirateShipSpawnerAuthoring authoring)
            {
                var spawner = GetEntity(authoring, TransformUsageFlags.None);
                
                AddComponent(spawner, new PirateShipSpawner
                {
                    ShipPrefab = GetEntity(authoring.Prefab, TransformUsageFlags.Dynamic),
                    NumberOfShips = authoring.SimulationConfig.NumberOfPirateShips,
                    SailingConstraints = authoring.SimulationConfig.SailingConstraints,
                });
            }
        }
    }
}