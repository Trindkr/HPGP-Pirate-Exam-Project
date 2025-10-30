using Components.Cannon;
using Unity.Burst;
using Unity.Entities;
using Unity.Physics;
using Unity.Transforms;

namespace Systems.Cannon
{
    public partial struct ShootingRangeSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
        
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
        
        }


        [BurstCompile]
        public partial struct FindShootingRange : IJobEntity
        {
            void Execute(ref CannonConstraints cannonConstraints, ref LocalToWorld shipTransform )
            {
                var raycastStart = shipTransform.Position;
                var raycastEnd = raycastStart + shipTransform.Forward * cannonConstraints.ShootingRange;
                
               
            }
        }
    }
}
