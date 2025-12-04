using Components;
using ScriptableObjects;
using Unity.Entities;
using UnityEngine;

namespace Authoring
{
    public class JobModeAuthoring : MonoBehaviour
    {
        [SerializeField] private SimulationConfig _simulationConfig;
        private class JobModeAuthoringBaker : Baker<JobModeAuthoring>
        {
            public override void Bake(JobModeAuthoring authoring)
            {
                var entity = GetEntity(authoring, TransformUsageFlags.None);
                AddComponent(entity, new JobModeSingleton
                {
                    JobMode = authoring._simulationConfig.JobMode,
                });
            }
        }
    }
}