using Components;
using Model;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Systems
{
    public static class ShipSpawnerHelper
    {
        public static void AddDefaultShipComponents(EntityCommandBuffer ecb, Entity prefab, SailingConstraints sailingConstraints, uint i, uint j)
        {
            var ship = ecb.Instantiate(prefab);
            var localTransform =
                LocalTransform.FromPosition(new float3(i * 10 + i * j / 3f, 0, j * 10 + i * j / 2f));
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
        }
    }
}