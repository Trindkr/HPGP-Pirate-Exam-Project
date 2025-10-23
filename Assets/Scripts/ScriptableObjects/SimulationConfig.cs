using Components;
using Model;
using UnityEngine;

namespace ScriptableObjects
{
    [CreateAssetMenu(fileName = "Simulation Config")]
    public class SimulationConfig : ScriptableObject
    {
        public int NumberOfPirateShips;
        public int NumberOfTradeShips;
        public SailingConstraints  SailingConstraints;
    }
}