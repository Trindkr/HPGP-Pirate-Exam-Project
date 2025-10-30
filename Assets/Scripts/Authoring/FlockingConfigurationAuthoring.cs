using Components;
using ScriptableObjects;
using Unity.Entities;
using UnityEngine;

namespace Authoring
{
    public class FlockingConfigurationAuthoring : MonoBehaviour
    {
        [SerializeField] private SimulationConfig _simulationConfig;

        private class FlockingConfigurationBaker : Baker<FlockingConfigurationAuthoring>
        {
            public override void Bake(FlockingConfigurationAuthoring authoring)
            {
                var entity = GetEntity(authoring, TransformUsageFlags.None);
                AddComponent(entity, new FlockingConfigurationSingleton
                {
                    FlockingConfiguration = authoring._simulationConfig.FlockingConfiguration,
                });
            }
        }
    }
}