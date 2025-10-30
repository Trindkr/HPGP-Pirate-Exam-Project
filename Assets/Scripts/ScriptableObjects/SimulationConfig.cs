using Model;
using UnityEngine;

namespace ScriptableObjects
{
    [CreateAssetMenu(fileName = "Simulation Config")]
    public class SimulationConfig : ScriptableObject
    {
        public int NumberOfPirateShips;
        public int NumberOfMerchantShips;
        [Min(1)]
        public int NumberOfMerchantFleets;
        [Min(1)]
        public int NumberOfPirateFleets;
        public SailingConstraints  SailingConstraints;
        public CannonConfiguration   CannonConfiguration;
        public FlockingConfiguration FlockingConfiguration;
    }
}