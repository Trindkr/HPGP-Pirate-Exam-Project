using Components;
using Model;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEditor.PackageManager;
using CannonConstraints = Components.CannonConstraints;

namespace Systems
{
    public static class ShipSpawnerHelper
    {
        public static void SpawnBoats(ref EntityCommandBuffer ecb, Entity shipPrefab, SailingConstraints sailingConstraints, Entity cannonballPrefab, Model.CannonConstraints cannonConstraints,
            int numberOfShips, uint2 startingOffset)
        {
            var xAmount = (uint)math.round(math.sqrt(numberOfShips));
            var zAmount = xAmount;
            for (uint z = 0; z < zAmount; z++)
            {
                for (uint x = 0; x < xAmount; x++)
                {
                    AddDefaultShipComponents(ref ecb, shipPrefab, sailingConstraints, cannonballPrefab, cannonConstraints, x, z, startingOffset);
                }
            }
        }

        private static void AddDefaultShipComponents(ref EntityCommandBuffer ecb,
            Entity prefab,
            SailingConstraints sailingConstraints,
            Entity cannonBallPrefab,
            Model.CannonConstraints cannonConstraints,
            uint x, uint z, uint2 startingOffset)
        {
            var ship = ecb.Instantiate(prefab);
            var localTransform =
                LocalTransform.FromPosition(new float3(x * 10 + x * z / 3f + startingOffset.x, 0, z * 10 + z * x / 2f + startingOffset.y));
            ecb.SetComponent(ship, localTransform);

            ecb.AddComponent(ship, new AngularMotion
            {
                MaxAcceleration = sailingConstraints.MaxAngularAcceleration,
                MaxSpeed = sailingConstraints.MaxAngularSpeed,
            });

            ecb.AddComponent(ship, new LinearMotion
            {
                MaxAcceleration = sailingConstraints.MaxLinearAcceleration,
                MaxSpeed = sailingConstraints.MaxLinearSpeed,
            });

            ecb.AddComponent<Navigation>(ship);

            ecb.AddComponent(ship, new CannonConstraints
            {
                ReloadTime = cannonConstraints.ReloadTime,
                ShootingForce = cannonConstraints.ShootingForce,
                ReloadTimer = UnityEngine.Random.Range(1f, cannonConstraints.ReloadTime),
                FireLeft = true
            });

            ecb.AddComponent(ship, new CannonballPrefab
            {
                Prefab = cannonBallPrefab
            });

        }
    }
}