using System.ComponentModel;
using System.Runtime.CompilerServices;
using Components;
using Components.Enum;
using Components.Fleet;
using Components.Tags;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace Systems
{
    partial struct MerchantTargetAssignmentSystem : ISystem
    {
        private const int OffsetMultiplier = 100;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<JobModeSingleton>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var merchants =
                SystemAPI.QueryBuilder()
                    .WithAll<Merchent>()
                    .WithNone<Sinking>()
                    .WithAll<LocalTransform>()
                    .Build().ToEntityArray(Allocator.TempJob);


            var pirateShipBuffer = SystemAPI.GetBufferLookup<FleetShipBuffer>(true);
            var merchantSeekerLookup = SystemAPI.GetComponentLookup<MerchantSeeker>(true);
            var sinkingLookup = SystemAPI.GetComponentLookup<Sinking>(true);
            
            var ecbParallel = new EntityCommandBuffer(Allocator.TempJob);

            var job = new TargetAssigningJob()
            {
                Merchants = merchants,
                FleetShipBufferLookup = pirateShipBuffer,
                MerchantSeekerLookup = merchantSeekerLookup,
                SinkingLookup = sinkingLookup,
                EntityCommandBuffer = ecbParallel.AsParallelWriter()
            };
            
            var jobModeSingleton = SystemAPI.GetSingleton<JobModeSingleton>();
            if (jobModeSingleton.JobMode == JobMode.Run)
            {
                var ecb = new EntityCommandBuffer(Allocator.Temp);
                
                foreach (var (_, _, pirateFleetEntity) in SystemAPI.Query<RefRO<Components.Fleet.Fleet>, RefRO<Pirate>>().WithEntityAccess())
                {
                    var ships = pirateShipBuffer[pirateFleetEntity].AsNativeArray();
                    foreach (var ship in ships)
                    {
                        var shipEntity = ship.ShipEntity;
                        if (merchantSeekerLookup.HasComponent(shipEntity))
                        {
                            var merchantSeeker = merchantSeekerLookup[shipEntity];

                            var hasValidTarget = !sinkingLookup.HasComponent(merchantSeeker.Target) &&
                                                 merchantSeeker.Target != default;
                            if (hasValidTarget)
                            {
                                return;
                            }

                            var newTargetIndex = (pirateFleetEntity.Index * OffsetMultiplier) % merchants.Length;

                            ecb.SetComponent(shipEntity, new MerchantSeeker() { Target = merchants[newTargetIndex] });
                        }
                    }

                    ships.Dispose();
                }
            }
            else if (jobModeSingleton.JobMode == JobMode.Schedule)
            {
                state.Dependency = job.Schedule(state.Dependency);
                state.Dependency.Complete();
            }
            else
            {
                state.Dependency = job.ScheduleParallel(state.Dependency);
                state.Dependency.Complete();
            }

            ecbParallel.Playback(state.EntityManager);
            ecbParallel.Dispose();
            merchants.Dispose();
        }

        [BurstCompile]
        private partial struct TargetAssigningJob : IJobEntity
        {
            public EntityCommandBuffer.ParallelWriter EntityCommandBuffer;
            [Unity.Collections.ReadOnly] public NativeArray<Entity> Merchants;
            [Unity.Collections.ReadOnly] public BufferLookup<FleetShipBuffer> FleetShipBufferLookup;
            [Unity.Collections.ReadOnly] public ComponentLookup<MerchantSeeker> MerchantSeekerLookup;
            [Unity.Collections.ReadOnly] public ComponentLookup<Sinking> SinkingLookup;

            void Execute([ChunkIndexInQuery] int chunkIndex, in Entity pirateFleetEntity, in Components.Fleet.Fleet fleet, in Pirate pirate)
            {
                var ships = FleetShipBufferLookup[pirateFleetEntity].AsNativeArray();
                foreach (var ship in ships)
                {
                    var shipEntity = ship.ShipEntity;
                    if (MerchantSeekerLookup.HasComponent(shipEntity))
                    {
                        var merchantSeeker = MerchantSeekerLookup[shipEntity];

                        var hasValidTarget = !SinkingLookup.HasComponent(merchantSeeker.Target) &&
                                             merchantSeeker.Target != default;
                        if (hasValidTarget)
                        {
                            return;
                        }

                        var newTargetIndex = (pirateFleetEntity.Index * OffsetMultiplier) % Merchants.Length;

                        EntityCommandBuffer.SetComponent(chunkIndex, shipEntity,
                            new MerchantSeeker() { Target = Merchants[newTargetIndex] });
                    }
                }

                ships.Dispose();
            }
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
        }
    }
}