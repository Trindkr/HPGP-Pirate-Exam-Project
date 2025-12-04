using Components.Enum;
using Model;
using UnityEngine;

namespace ScriptableObjects
{
    public enum ShipAmountOptions
    {
        _1000 = 1000,
        _10000 = 10000,
        _20000 = 20000,
    }
    
    [CreateAssetMenu(fileName = "Simulation Config")]
    public class SimulationConfig : ScriptableObject
    {
        [SerializeField] private ShipAmountOptions _shipAmountOptions;
        public int ShipAmount => (int) _shipAmountOptions;
        public JobMode JobMode;
        public ObstacleAvoidanceMode ObstacleAvoidanceMode;
        [Range(1, 100)]
        public int FleetSize = 15;
        public SailingConstraints  SailingConstraints;
        public CannonConfiguration   CannonConfiguration;
        public FlockingConfiguration FlockingConfiguration;
    }
}