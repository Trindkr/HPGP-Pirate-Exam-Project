using Components;
using Components.Tags;
using Monobehaviours;
using ScriptableObjects;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Serialization;

namespace Authoring
{
    public class FirstPersonShipAuthoring : MonoBehaviour
    {
        public GameObject Prefab;
        public SimulationConfig SimulationConfig;
        public FirstPersonCamera FirstPersonCamera;
        
        private class FirstPersonShipBaker : Baker<FirstPersonShipAuthoring>
        {
            public override void Bake(FirstPersonShipAuthoring authoring)
            {
                var firstPersonShip = GetEntity(authoring, TransformUsageFlags.Dynamic);
                AddComponent(firstPersonShip, new LocalTransform
                {
                    Position = new float3(-5f, 0, -5f),
                    Rotation = quaternion.identity,
                    Scale = 1f
                });
                
                AddComponent(firstPersonShip, new AngularMotion
                {
                    MaxAcceleration = authoring.SimulationConfig.MaxAngularAcceleration,
                    MaxSpeed = authoring.SimulationConfig.MaxAngularSpeed,
                });

                AddComponent(firstPersonShip, new LinearMotion
                {
                    MaxAcceleration = authoring.SimulationConfig.MaxLinearAcceleration,
                    MaxSpeed = authoring.SimulationConfig.MaxLinearSpeed,
                });

                AddComponent<Navigation>(firstPersonShip);
                AddComponent<FirstPersonTag>(firstPersonShip);
            }
        }
    }
}