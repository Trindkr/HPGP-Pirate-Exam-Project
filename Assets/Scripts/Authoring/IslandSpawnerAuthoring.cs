using Components;
using ScriptableObjects;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Authoring
{
    public class IslandSpawnerAuthoring : MonoBehaviour
    {
        [SerializeField] private GameObject _islandPrefab;
        [SerializeField] private float _baseRadius = 100f;
        [SerializeField] private SimulationConfig _simulationConfig;

        private class IslandSpawnerBaker : Baker<IslandSpawnerAuthoring>
        {
            public override void Bake(IslandSpawnerAuthoring authoring)
            {
                var radiusMultiplier = math.log10(authoring._simulationConfig.ShipAmount);
                Entity entity = GetEntity(authoring, TransformUsageFlags.None);
                AddComponent(entity, new IslandSpawner
                {
                    IslandAmount = (int) math.floor(2 * radiusMultiplier),
                    IslandPrefab = GetEntity(authoring._islandPrefab, TransformUsageFlags.WorldSpace),
                    Radius = authoring._baseRadius * radiusMultiplier * 2f,
                });
            }
        }
    }
}