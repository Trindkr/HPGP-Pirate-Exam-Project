using System.ComponentModel;
using System.Runtime.CompilerServices;
using Components;
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
            
            var ecb = new EntityCommandBuffer(Allocator.TempJob);

            var job = new TargetAssigningJob()
            {
                Merchants = merchants,
                FleetShipBufferLookup = pirateShipBuffer,
                MerchantSeekerLookup = merchantSeekerLookup,
                SinkingLookup = sinkingLookup,
                EntityCommandBuffer = ecb.AsParallelWriter()
            };

            state.Dependency = job.ScheduleParallel(state.Dependency);
            state.Dependency.Complete();
            
            ecb.Playback(state.EntityManager);
            ecb.Dispose();  
            
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