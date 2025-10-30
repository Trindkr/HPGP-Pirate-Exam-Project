using System.Runtime.CompilerServices;
using Components.Fleet;
using Model;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;

namespace Systems.Fleet
{
    public partial struct FleetSpawnerSystem : ISystem, ISystemStartStop
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>();
        }
        
        public void OnStartRunning(ref SystemState state)
        {
            Random random = new Random(42);
            var ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
            var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);
            foreach (var spawner in SystemAPI.Query<RefRO<FleetSpawner>>())
            {
                for (int i = 0; i < spawner.ValueRO.NumberOfPirateFleets; i++)
                {
                    var offset = RandomPointOnUnitCircle(ref random) * (i+1) * 30;
                    SpawnFleet(
                        ref ecb, 
                        spawner.ValueRO.PirateShipsPerFleet, 
                        spawner.ValueRO.PirateShipPrefab, 
                        spawner.ValueRO.SailingConstraints,
                        spawner.ValueRO.CannonConfiguration,
                        spawner.ValueRO.CannonballPrefab,
                        offset);
                }
                
                for (int i = 0; i < spawner.ValueRO.NumberOfMerchantFleets; i++)
                {
                    var offset = RandomPointOnUnitCircle(ref random) * (i+1) * 30;
                    SpawnFleet(
                        ref ecb, 
                        spawner.ValueRO.MerchantShipsPerFleet, 
                        spawner.ValueRO.MerchantShipPrefab, 
                        spawner.ValueRO.SailingConstraints,
                        spawner.ValueRO.CannonConfiguration,
                        spawner.ValueRO.CannonballPrefab,
                        offset);
                }
            }
        }
        
        [BurstCompile, MethodImpl(MethodImplOptions.AggressiveInlining)]
        float2 RandomPointOnUnitCircle(ref Unity.Mathematics.Random random)
        {
            float angle = random.NextFloat(0f, math.PI2);
            return new float2(math.cos(angle), math.sin(angle));
        }
        
        [BurstCompile, MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void SpawnFleet(
            ref EntityCommandBuffer ecb,
            int fleetSize, 
            Entity shipPrefab, 
            SailingConstraints sailingConstraints,
            Model.CannonConfiguration cannonConfiguration,
            Entity cannonballPrefab,
            float2 offset) 
        {
            var fleetEntity = ecb.CreateEntity();
            
            ecb.AddComponent(fleetEntity, new Components.Fleet.Fleet());
            
            var buffer = ecb.AddBuffer<FleetShipBuffer>(fleetEntity);

            var xAmount = (uint) math.round(math.sqrt(fleetSize));
            var zAmount = xAmount;
            for (int z = 0; z < zAmount; z++)
            {
                for (int x = 0; x < xAmount; x++)
                {
                    var position = new float3(x * 10 + x * z / 3f + offset.x, 0,
                        z * 10 + z * x / 2f + offset.y);
                    var shipEntity = ShipSpawnerHelper.AddDefaultShipComponents(
                        ref ecb,
                        shipPrefab,
                        sailingConstraints,
                        cannonballPrefab,
                        cannonConfiguration,
                        position);
                    ecb.AddComponent(shipEntity, new FleetMember
                    {
                        FleetEntity = fleetEntity
                    });
                
                    buffer.Add(new FleetShipBuffer { ShipEntity = shipEntity });
                }
            }
        }
        
        

        public void OnStopRunning(ref SystemState state)
        {
        }
    }
}