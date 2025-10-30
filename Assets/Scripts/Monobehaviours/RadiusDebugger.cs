using System;
using UnityEngine;

namespace Monobehaviours
{
    public class RadiusDebugger : MonoBehaviour
    {
        [SerializeField] private float _radius;
        private void OnDrawGizmos() => Gizmos.DrawWireSphere(transform.position, _radius);
    }
}