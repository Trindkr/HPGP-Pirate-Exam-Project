using Components;
using Model;
using UnityEngine;
using UnityEngine.Serialization;

namespace ScriptableObjects
{
    [CreateAssetMenu(fileName = "Simulation Config")]
    public class SimulationConfig : ScriptableObject
    {
        public int NumberOfPirateShips;
        public int NumberOfMerchants;
        public SailingConstraints  SailingConstraints;
        public CannonConstraints   CannonConstraints;
    }
}