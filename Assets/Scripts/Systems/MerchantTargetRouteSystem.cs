using Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine.SocialPlatforms;

namespace Systems
{
    [UpdateAfter(typeof(MerchantTargetAssignmentSystem))]
    public partial struct MerchantTargetRouteSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var transformLookup = SystemAPI.GetComponentLookup<LocalTransform>(true);
            var targetJob = new MerchantTargetJob()
            {
                LocalTransformLookup = transformLookup
            };
            state.Dependency = targetJob.ScheduleParallel(state.Dependency);
        }
        
        [BurstCompile]
        public partial struct MerchantTargetJob : IJobEntity
        {
            [ReadOnly] public ComponentLookup<LocalTransform> LocalTransformLookup;
            public void Execute(in LocalTransform localTransform,  in MerchantSeeker merchantSeeker, ref Navigation navigation)
            {
                var merchantTargetPosition = LocalTransformLookup[merchantSeeker.Target].Position;
                
                var targetDirection = merchantTargetPosition - localTransform.Position;
                navigation.DesiredDirection += math.normalize(targetDirection) * 100f;
            }
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {

        }
    }
}