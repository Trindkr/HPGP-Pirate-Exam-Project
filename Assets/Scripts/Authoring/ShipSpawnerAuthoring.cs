using Components;
using ScriptableObjects;
using Unity.Entities;
using UnityEngine;

namespace Authoring
{
    public class ShipSpawnerAuthoring : MonoBehaviour
    {
        [SerializeField] private GameObject _prefab;
        [SerializeField] private SimulationConfig _simulationConfig;
        
        private class ShipSpawnerBaker : Baker<ShipSpawnerAuthoring>
        {
            public override void Bake(ShipSpawnerAuthoring authoring)
            {
                var entity = GetEntity(authoring, TransformUsageFlags.None);
                AddComponent(entity, new ShipSpawner
                {
                    ShipPrefab = GetEntity(authoring._prefab, TransformUsageFlags.Dynamic),
                    NumberOfShips = authoring._simulationConfig.NumberOfShips
                });
            }
        }
    }
}