using Components;
using ScriptableObjects;
using Unity.Entities;
using UnityEngine;

namespace Authoring
{
    public class TradeShipSpawnerAuthoring : MonoBehaviour
    {
        public GameObject Prefab;
        public SimulationConfig SimulationConfig;
        
        private class TradeShipSpawnerBaker : Baker<TradeShipSpawnerAuthoring>
        {
            public override void Bake(TradeShipSpawnerAuthoring authoring)
            {
                var spawner = GetEntity(authoring, TransformUsageFlags.None);
                
                AddComponent(spawner, new TradeShipSpawner
                {
                    ShipPrefab = GetEntity(authoring.Prefab, TransformUsageFlags.Dynamic),
                    NumberOfShips = authoring.SimulationConfig.NumberOfPirateShips,
                    SailingConstraints = authoring.SimulationConfig.SailingConstraints,
                });
            }
        }
    }
}