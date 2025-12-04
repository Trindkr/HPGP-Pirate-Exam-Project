using Components;
using Components.Enum;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.SocialPlatforms;

namespace Systems
{
    [UpdateAfter(typeof(MerchantTargetAssignmentSystem)), UpdateBefore(typeof(MoveSystem))]
    public partial struct MerchantTargetRouteSystem : ISystem
    {
        //[BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<JobModeSingleton>();
        }

        //[BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var transformLookup = SystemAPI.GetComponentLookup<LocalTransform>(true);
            var job = new MerchantTargetJob()
            {
                LocalTransformLookup = transformLookup
            };
            
            var jobModeSingleton = SystemAPI.GetSingleton<JobModeSingleton>();
            if (jobModeSingleton.JobMode == JobMode.Run)
            {
                job.Run();
            }
            else if (jobModeSingleton.JobMode == JobMode.Schedule)
            {
                state.Dependency = job.Schedule(state.Dependency);
            }
            else
            {
                state.Dependency = job.ScheduleParallel(state.Dependency);
            }

        }
        
        //[BurstCompile]
        public partial struct MerchantTargetJob : IJobEntity
        {
            [ReadOnly] public ComponentLookup<LocalTransform> LocalTransformLookup;
            public void Execute(in LocalTransform localTransform, in MerchantSeeker merchantSeeker, ref Navigation navigation)
            {
                if (!LocalTransformLookup.HasComponent(merchantSeeker.Target))
                {
                    //Debug.DrawRay(localTransform.Position, new float3(0, 50, 0), Color.red);
                    return;
                }

                var merchantTargetPosition = LocalTransformLookup[merchantSeeker.Target].Position;

                var targetDirection = merchantTargetPosition - localTransform.Position;
                navigation.DesiredDirection += math.normalize(targetDirection) * 10f;
                
                //Debug.DrawLine(localTransform.Position, merchantTargetPosition, Color.green);
            }

        }

        //[BurstCompile]
        public void OnDestroy(ref SystemState state)
        {

        }
    }
}