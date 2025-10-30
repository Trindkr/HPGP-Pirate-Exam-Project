using Components;
using Components.Enum;
using Components.Tags;
using Model;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using CannonConstraints = Components.Cannon.CannonConstraints;

namespace Systems
{
    public static class ShipSpawnerHelper
    {
        public static void SpawnBoats(ref EntityCommandBuffer ecb, Entity shipPrefab, SailingConstraints sailingConstraints, Entity cannonballPrefab, Model.CannonConstraintsConfig cannonConstraintsConfig,
            int numberOfShips, uint2 startingOffset)
        {
            var xAmount = (uint)math.round(math.sqrt(numberOfShips));
            var zAmount = xAmount;
            for (uint z = 0; z < zAmount; z++)
            {
                for (uint x = 0; x < xAmount; x++)
                {
                    var position = new float3(x * 10 + x * z / 3f + startingOffset.x, 0,
                        z * 10 + z * x / 2f + startingOffset.y);
                    var ship = AddDefaultShipComponents(ref ecb, shipPrefab, sailingConstraints, cannonballPrefab, cannonConstraintsConfig,  position);
                    ecb.AddComponent(ship, new AllFlockingTag());
                }
            }
        }
        
        public static Entity AddDefaultShipComponents(ref EntityCommandBuffer ecb,
            Entity prefab,
            SailingConstraints sailingConstraints,
            Entity cannonBallPrefab,
            Model.CannonConstraintsConfig cannonConstraintsConfig,
            float3 position)
        {
            var ship = ecb.Instantiate(prefab);
            var localTransform =
                LocalTransform.FromPosition(position);
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
            
            ecb.AddComponent(ship, CreateCannonConstraintsComponent(cannonConstraintsConfig));

            ecb.AddComponent(ship, new CannonballPrefab
            {
                Prefab = cannonBallPrefab,
            });

            return ship;
        }

        private static CannonConstraints  CreateCannonConstraintsComponent(CannonConstraintsConfig cannonConstraintsConfig)
        {
            var shootingForce = UnityEngine.Random.Range(cannonConstraintsConfig.MinShootingForce, cannonConstraintsConfig.MaxShootingForce);
            var shootingAngle = UnityEngine.Random.Range(cannonConstraintsConfig.MinShootingAngle, cannonConstraintsConfig.MaxShootingAngle);
            const float gravity = 9.81f;

            var shootingRange = (shootingForce * shootingForce * math.sin(2f * math.radians(shootingAngle))) / gravity;

            return new CannonConstraints
            {
                ReloadTime = cannonConstraintsConfig.ReloadTime,
                ShootingAngle = shootingAngle,
                ShootingForce = shootingForce,
                ShootingRange = shootingRange,
                ReloadTimer = UnityEngine.Random.Range(1f, cannonConstraintsConfig.ReloadTime),
                ShootingDirection = ShootingDirection.Left
            };
        }
    }
}