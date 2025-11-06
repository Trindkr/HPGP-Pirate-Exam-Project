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
        public static Entity AddDefaultShipComponents(ref EntityCommandBuffer ecb,
            Entity prefab,
            SailingConstraints sailingConstraints,
            Entity cannonBallPrefab,
            Model.CannonConfiguration cannonConfiguration,
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

            ecb.AddComponent<Ship>(ship);
            
            ecb.AddComponent(ship, CreateCannonConstraintsComponent(cannonConfiguration));

            ecb.AddComponent(ship, new CannonballPrefab
            {
                Prefab = cannonBallPrefab,
            });

            return ship;
        }

        private static CannonConstraints  CreateCannonConstraintsComponent(CannonConfiguration cannonConfiguration)
        {
            var shootingForce = UnityEngine.Random.Range(cannonConfiguration.MinShootingForce, cannonConfiguration.MaxShootingForce);
            var shootingAngle = UnityEngine.Random.Range(cannonConfiguration.MinShootingAngle, cannonConfiguration.MaxShootingAngle);
            const float gravity = 9.81f;

            var shootingRange = (shootingForce * shootingForce * math.sin(2f * math.radians(shootingAngle))) / gravity;

            return new CannonConstraints
            {
                ReloadTime = cannonConfiguration.ReloadTime,
                ShootingAngle = shootingAngle,
                ShootingForce = shootingForce,
                ShootingRange = shootingRange,
                ReloadTimer = UnityEngine.Random.Range(1f, cannonConfiguration.ReloadTime),
                ShootingDirection = ShootingDirection.None
            };
        }
    }
}