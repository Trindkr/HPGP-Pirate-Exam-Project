using UnityEngine;

namespace ScriptableObjects
{
    [CreateAssetMenu(fileName = "Simulation Config")]
    public class SimulationConfig : ScriptableObject
    {
        public int NumberOfPirateShips;
        public int NumberOfTradeShips;
        public float MaxAngularAcceleration;
        public float MaxAngularSpeed;
        public float MaxLinearAcceleration;
        public float MaxLinearSpeed;
    }
}