using UnityEngine;

namespace ScriptableObjects
{
    [CreateAssetMenu(fileName = "Simulation Config")]
    public class SimulationConfig : ScriptableObject
    {
        public int NumberOfShips;
    }
}