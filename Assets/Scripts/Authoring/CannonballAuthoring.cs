using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using UnityEngine;

namespace Authoring
{
    public class CannonballAuthoring : MonoBehaviour
    {
        public float Radius = 0.5f;
        public float Mass = 1f;

        private class CannonballAuthoringBaker : Baker<CannonballAuthoring>
        {
            public override void Bake(CannonballAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                var sphere = new SphereGeometry { Center = float3.zero, Radius = authoring.Radius };
                var collider = Unity.Physics.SphereCollider.Create(sphere);

                AddComponent(entity, new PhysicsCollider { Value = collider });

                AddComponent(entity, PhysicsMass.CreateDynamic(MassProperties.UnitSphere, authoring.Mass));

                AddComponent(entity, new PhysicsVelocity());

                AddComponent(entity, new PhysicsGravityFactor { Value = 1f });

                AddComponent<CannonballTag>(entity);

                AddComponent(entity, new DespawnBelowYLevel { YLevel = -10f });

                AddComponent<Prefab>(entity);
            }
        }

    }

}