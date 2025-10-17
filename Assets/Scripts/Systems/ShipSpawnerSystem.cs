using Components;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Systems
{
    [BurstCompile]
    public partial struct ShipSpawnerSystem : ISystem, ISystemStartStop
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>();
        }

        public void OnStartRunning(ref SystemState state)
        {
            var ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
            var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);
            SpawnBoats(ref state, ref ecb);
        }

        public void OnStopRunning(ref SystemState state)
        {
        }

        private void SpawnBoats(ref SystemState state, ref EntityCommandBuffer ecb)
        {
            foreach (var spawner in SystemAPI.Query<RefRO<ShipSpawner>>())
            {
                for (int j = 0; j < spawner.ValueRO.NumberOfShips; j++)
                {
                    for (uint i = 0; i < spawner.ValueRO.NumberOfShips; i++)
                    {
                        var ship = ecb.Instantiate(spawner.ValueRO.ShipPrefab);
                        var localTransform = LocalTransform.FromPosition(new float3(i * 5, 0, j * 5));
                        ecb.SetComponent(ship, localTransform);

                        // ecb.AddComponent(ship, new Ship
                        // {
                        //     Random = Random.CreateFromIndex(i),
                        //     Speed = 1.0f,
                        //     MaxTurningSpeed = 0.2f,
                        //     AngularVelocity = 0f
                        // });

                        ecb.AddComponent(ship, new AngularMotion
                        {
                            MaxAcceleration = spawner.ValueRO.MaxAngularAcceleration,
                            MaxSpeed = spawner.ValueRO.MaxAngularSpeed,
                        });

                        ecb.AddComponent(ship, new LinearMotion
                        {
                            MaxAcceleration = spawner.ValueRO.MaxLinearAcceleration,
                            MaxSpeed = spawner.ValueRO.MaxLinearSpeed,
                        });

                        ecb.AddComponent(ship, new Navigation());
                    }
                }
            }
        }
    }
}