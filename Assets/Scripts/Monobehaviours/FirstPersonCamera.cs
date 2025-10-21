using System;
using Components.Tags;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace Monobehaviours
{
    public class FirstPersonCamera : MonoBehaviour
    {
        private Entity _entity;
        private EntityManager _entityManager;

        public Vector3 Position;
        
        private void Start()
        {
            _entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            var query = _entityManager.CreateEntityQuery(typeof(FirstPersonTag), typeof(LocalTransform));

            if (query.CalculateEntityCount() > 0)
            {
                _entity = query.GetSingletonEntity(); // assume only one
            }
        }

        private void Update()
        {
            if (_entity != Entity.Null && _entityManager.HasComponent<LocalTransform>(_entity))
            {
                var firstPersonTransform = _entityManager.GetComponentData<LocalTransform>(_entity);
                Position = firstPersonTransform.Position;
            }
        }
    }
}