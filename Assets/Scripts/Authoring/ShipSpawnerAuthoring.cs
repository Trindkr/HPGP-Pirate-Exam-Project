using Components;
using ScriptableObjects;
using Unity.Entities;
using UnityEngine;

namespace Authoring
{
    public class ShipSpawnerAuthoring : MonoBehaviour
    {
        public GameObject Prefab;
        public SimulationConfig SimulationConfig;
        
        private class ShipSpawnerBaker : Baker<ShipSpawnerAuthoring>
        {
            public override void Bake(ShipSpawnerAuthoring authoring)
            {
                var spawner = GetEntity(authoring, TransformUsageFlags.None);
                
                AddComponent(spawner, new ShipSpawner
                {
                    ShipPrefab = GetEntity(authoring.Prefab, TransformUsageFlags.Dynamic),
                    NumberOfShips = authoring.SimulationConfig.NumberOfShips,
                    MaxAngularAcceleration = authoring.SimulationConfig.MaxAngularAcceleration,
                    MaxAngularSpeed = authoring.SimulationConfig.MaxAngularSpeed,
                    MaxLinearAcceleration = authoring.SimulationConfig.MaxLinearAcceleration,
                    MaxLinearSpeed = authoring.SimulationConfig.MaxLinearSpeed
                });
            }
        }
    }
}