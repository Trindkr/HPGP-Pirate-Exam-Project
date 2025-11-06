using Components;
using Components.Tags;
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
        public static Entity AddDefaultShipComponents(ref EntityCommandBuffer ecb,
            Entity prefab,
            SailingConstraints sailingConstraints,
            Entity cannonBallPrefab,
            Model.CannonConstraints cannonConstraints,
            float3 position)
        {
            Random random = new Random((uint) position.x + (uint) position.z);
            var ship = ecb.Instantiate(prefab);
            var localTransform =
                LocalTransform.FromPosition(position);
            ecb.SetComponent(ship, localTransform);

            ecb.AddComponent(ship, new AngularMotion
            {
                MaxAcceleration = sailingConstraints.MaxAngularAcceleration,
                MaxSpeed = sailingConstraints.MaxAngularSpeed,
            });

            float maxSpeed = sailingConstraints.MaxLinearSpeed; 
                //random.NextFloat(sailingConstraints.MaxLinearSpeed - .5f, sailingConstraints.MaxLinearSpeed + .5f);
            ecb.AddComponent(ship, new LinearMotion
            {
                MaxAcceleration = sailingConstraints.MaxLinearAcceleration,
                MaxSpeed = maxSpeed,
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
                Prefab = cannonBallPrefab,
            });

            return ship;
        }
    }
}