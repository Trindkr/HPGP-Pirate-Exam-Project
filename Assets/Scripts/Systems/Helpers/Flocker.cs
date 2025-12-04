using System.Runtime.CompilerServices;
using Components;
using ExtensionMethods;
using Model;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Transforms;

namespace Systems.Helpers
{
    //[BurstCompile]
    public static class Flocker
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Flock(
            ref Navigation navigation,
            in LocalTransform localTransform,
            in NativeArray<LocalTransform> fleetMembers,
            in FlockingConfiguration flockingConfiguration)
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
                if (squareDistance == 0f) 
                    continue;

                nearbyCount++;
                cohesion += offset;
                alignment += other.Forward().xz;

                // gør det samme med cohesion og alignment, så skibe som er tæt på vægter højere
                separation -= offset * (1.0f / squareDistance);
            }

            float inverseNearbyCount = 1f / nearbyCount;
            cohesion *= inverseNearbyCount;
            alignment *= inverseNearbyCount;

            // make config file for this
            float alignmentStrength = flockingConfiguration.AlignmentStrength;
            float cohesionStrength = flockingConfiguration.CohesionStrength;
            float separationStrength = flockingConfiguration.SeparationStrength;

            float2 target = alignment * alignmentStrength + 
                            cohesion * cohesionStrength +
                            separation * separationStrength;
            if(math.lengthsq(target) < float.Epsilon)
                return;
            
            navigation.DesiredDirection = math.normalize(target.x0z());
            var magnitudeSquared = math.lengthsq(target.x0z());
            navigation.DesiredMoveSpeed = magnitudeSquared;
        }
    }
}