using UnityEngine;

namespace ScriptableObjects
{
    [CreateAssetMenu(fileName = "Simulation Config")]
    public class SimulationConfig : ScriptableObject
    {
        public int NumberOfShips;
        public float MaxAngularAcceleration;
        public float MaxAngularSpeed;
        public float MaxLinearAcceleration;
        public float MaxLinearSpeed;
    }
}