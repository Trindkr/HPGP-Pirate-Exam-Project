using Components;
using Components.Tags;
using ScriptableObjects;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Authoring
{
    public class FirstPersonShipAuthoring : MonoBehaviour
    {
        public GameObject Prefab;
        public SimulationConfig SimulationConfig;
        
        private class FirstPersonShipBaker : Baker<FirstPersonShipAuthoring>
        {
            public override void Bake(FirstPersonShipAuthoring authoring)
            {
                var firstPersonShip = GetEntity(authoring, TransformUsageFlags.Dynamic);
                AddComponent(firstPersonShip, new LocalTransform
                {
                    Position = authoring.transform.position,
                    Rotation = quaternion.identity,
                    Scale = 1f
                });
                
                AddComponent(firstPersonShip, new AngularMotion
                {
                    MaxAcceleration = authoring.SimulationConfig.SailingConstraints.MaxAngularAcceleration,
                    MaxSpeed = authoring.SimulationConfig.SailingConstraints.MaxAngularSpeed,
                });

                AddComponent(firstPersonShip, new LinearMotion
                {
                    MaxAcceleration = authoring.SimulationConfig.SailingConstraints.MaxLinearAcceleration,
                    MaxSpeed = authoring.SimulationConfig.SailingConstraints.MaxLinearSpeed,
                });

                AddComponent<Navigation>(firstPersonShip);
                AddComponent<FirstPersonTag>(firstPersonShip);
            }
        }
    }
}