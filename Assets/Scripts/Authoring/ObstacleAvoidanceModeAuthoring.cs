using Components;
using ScriptableObjects;
using Unity.Entities;
using UnityEngine;

namespace Authoring
{
    public class ObstacleAvoidanceModeAuthoring : MonoBehaviour
    {
        [SerializeField] private SimulationConfig _simulationConfig;
        
        private class ObstacleAvoidanceModeAuthoringBaker : Baker<ObstacleAvoidanceModeAuthoring>
        {
            public override void Bake(ObstacleAvoidanceModeAuthoring authoring)
            {
                var entity = GetEntity(authoring, TransformUsageFlags.None);
                AddComponent(entity, new ObstacleAvoidanceModeSingleton
                {
                    ObstacleAvoidanceMode = authoring._simulationConfig.ObstacleAvoidanceMode
                });
            }
        }
    }
}