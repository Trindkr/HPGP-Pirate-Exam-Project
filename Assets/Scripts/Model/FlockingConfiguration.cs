using System;
using UnityEngine;

namespace Model
{
    [Serializable]
    public struct FlockingConfiguration
    {
        [Min(0)]
        public float CohesionStrength;
        [Min(0)]
        public float AlignmentStrength;
        [Min(0)]
        public float SeparationStrength;
    }
}