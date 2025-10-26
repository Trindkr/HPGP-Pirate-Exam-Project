using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Transforms;

namespace Systems.Helpers
{
    [BurstCompile]
    public class Flocker
    {
        public NativeArray<LocalTransform> FlockMembers { get; }
        public float MaxDistance { get; }
        
        public void Flock(ref LocalTransform localTransform, in NativeArray<LocalTransform> fleetMembers)
        {
            float2 myPosition = localTransform.Position.xz;

            var nearbyCount = 1;
            var cohesion = new float2();
            var alignment = new float2();
            var separation = new float2();

            foreach (LocalTransform other in fleetMembers)
            {
                float2 otherPosition = other.Position.xz;
                float2 offset = otherPosition - myPosition;
                float squareDistance = math.lengthsq(offset);
                if (squareDistance is > MaxDistance or 0f or float.NaN) continue;

                nearbyCount++;
                cohesion += offset;
                alignment += other.Forward().xz;

                // gør det samme med cohesion og alignment, så skiber som er tæt på vægter højere
                separation -= offset * (1.0f / squareDistance - 1.0f / MaxDistance);
            }

            float inverseNearbyCount = 1f / nearbyCount;
            cohesion *= inverseNearbyCount;
            alignment *= inverseNearbyCount;

            // make config file for this
            const float alignmentStrength = 7f;
            const float cohesionStrength = .8f;
            const float separationStrength = 50f;

            float2 target = alignment * alignmentStrength + 
                            cohesion * cohesionStrength +
                            separation * separationStrength;

            navigation.DesiredDirection = new float3(target.x, 0, target.y);
            var magnitudeSquared = math.lengthsq(navigation.DesiredDirection);
            navigation.DesiredMoveSpeed = magnitudeSquared;
        }
    }
}