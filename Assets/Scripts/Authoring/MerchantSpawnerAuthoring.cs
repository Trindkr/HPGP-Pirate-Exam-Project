using Components;
using ScriptableObjects;
using Unity.Entities;
using UnityEngine;

namespace Authoring
{
    public class MerchantSpawnerAuthoring : MonoBehaviour
    {
        public GameObject Prefab;
        public SimulationConfig SimulationConfig;
        
        private class MerchantSpawnerBaker : Baker<MerchantSpawnerAuthoring>
        {
            public override void Bake(MerchantSpawnerAuthoring authoring)
            {
                var spawner = GetEntity(authoring, TransformUsageFlags.None);
                
                AddComponent(spawner, new MerchantSpawner
                {
                    ShipPrefab = GetEntity(authoring.Prefab, TransformUsageFlags.Dynamic),
                    NumberOfShips = authoring.SimulationConfig.NumberOfPirateShips,
                    SailingConstraints = authoring.SimulationConfig.SailingConstraints,
                });
            }
        }
    }
}