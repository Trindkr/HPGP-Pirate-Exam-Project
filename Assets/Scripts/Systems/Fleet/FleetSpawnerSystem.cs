using System.Runtime.CompilerServices;
using Components;
using Components.Enum;
using Components.Fleet;
using Components.Tags;
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
                float k = 30f;
                int merchantFleetAmount = spawner.ValueRO.MerchantShipAmount/spawner.ValueRO.MerchantShipsPerFleet;
                for (int i = 0; i < merchantFleetAmount; i++)
                {
                    var offset = RandomPointOnUnitCircle(ref random) * i * k / math.pow(i+1f, 0.5f);
                    SpawnFleet(
                        ref ecb,
                        FactionType.Merchant,
                        spawner.ValueRO.MerchantShipsPerFleet,
                        spawner.ValueRO.MerchantShipPrefab,
                        spawner.ValueRO.SailingConstraints,
                        spawner.ValueRO.CannonConfiguration,
                        spawner.ValueRO.CannonballPrefab,
                        offset,
                        i % 5);
                }
                
                int pirateFleetAmount = spawner.ValueRO.PirateShipAmount/spawner.ValueRO.PirateShipsPerFleet;
                for (int i = 0; i < pirateFleetAmount; i++)
                {
                    var offset = RandomPointOnUnitCircle(ref random) * i * k / math.pow(i+1f, 0.5f);
                    SpawnFleet(
                        ref ecb,
                        FactionType.Pirate,
                        spawner.ValueRO.PirateShipsPerFleet,
                        spawner.ValueRO.PirateShipPrefab,
                        spawner.ValueRO.SailingConstraints,
                        spawner.ValueRO.CannonConfiguration,
                        spawner.ValueRO.CannonballPrefab,
                        offset,
                        i % 5);
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
            FactionType factionType,
            int fleetSize,
            Entity shipPrefab,
            SailingConstraints sailingConstraints,
            CannonConfiguration cannonConfiguration,
            Entity cannonballPrefab,
            float2 offset,
            int islandIndex)
        {
            var fleetEntity = ecb.CreateEntity();

            ecb.AddComponent(fleetEntity, new Components.Fleet.Fleet());

            var buffer = ecb.AddBuffer<FleetShipBuffer>(fleetEntity);

            if (factionType == FactionType.Merchant)
            {
                ecb.AddComponent<Merchent>(fleetEntity);
            }
            else
            {
                ecb.AddComponent<Pirate>(fleetEntity);
            }

            var xAmount = (int)math.ceil(math.sqrt(fleetSize));
            var zAmount = xAmount;
            for (int z = 0; z < zAmount; z++)
            {
                for (int x = 0; x < xAmount; x++)
                {
                    var currentShipIndex = z * zAmount + x;
                    if (currentShipIndex >= fleetSize)
                        return;
                    var position = new float3(x * 10 + x * z + offset.x, 0,
                        z * 10 + z * x + offset.y);
                    
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

                    ecb.AddComponent(shipEntity, new Faction
                    {
                        Value = factionType
                    });

                    if (factionType == FactionType.Merchant)
                    {
                        ecb.AddComponent(shipEntity, new IslandSeeker()
                        {
                            IslandIndex = islandIndex
                        });
                        
                        ecb.AddComponent<Merchent>(shipEntity);
                    }
                    else
                    {
                        ecb.AddComponent(shipEntity, new MerchantSeeker()
                        {
                            Target = default
                        });
                        
                        ecb.AddComponent<Pirate>(shipEntity);

                    }


                    buffer.Add(new FleetShipBuffer { ShipEntity = shipEntity });
                }
            }
        }


        public void OnStopRunning(ref SystemState state)
        {
        }
    }
}