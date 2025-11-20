using Components;
using Unity.Entities;
using UnityEngine;

namespace Authoring
{
    public class IslandSpawnerAuthoring : MonoBehaviour
    {
        [SerializeField] private GameObject _islandPrefab;

        private class IslandSpawnerBaker : Baker<IslandSpawnerAuthoring>
        {
            public override void Bake(IslandSpawnerAuthoring authoring)
            {
                Entity entity = GetEntity(authoring, TransformUsageFlags.None);
                AddComponent(entity, new IslandSpawner
                {
                    IslandAmount = 5,
                    IslandPrefab = GetEntity(authoring._islandPrefab, TransformUsageFlags.WorldSpace),
                    Radius = 150f,
                });
            }
        }
    }
}