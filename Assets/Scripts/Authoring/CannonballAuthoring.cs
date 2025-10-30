using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;

namespace Authoring
{
    public class CannonballAuthoring : MonoBehaviour
    {
        //public GameObject Prefab;

        private class CannonballAuthoringBaker : Baker<CannonballAuthoring>
        {
            public override void Bake(CannonballAuthoring authoring)
            {
                var entity = GetEntity(authoring, TransformUsageFlags.Dynamic);
                
                AddComponent<CannonballTag>(entity);

                AddComponent(entity, new DespawnBelowYLevel { YLevel = -10f });

                AddComponent<Prefab>(entity);
            }
        }

    }

}