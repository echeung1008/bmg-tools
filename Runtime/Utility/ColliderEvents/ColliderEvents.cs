using System;
using UnityEngine;

namespace BlueMuffinGames.Utility
{
    [RequireComponent(typeof(Collider))]
    public class ColliderEvents : MonoBehaviour
    {
        public event Action<Collision> CollisionEnter = delegate {};
        public event Action<Collision> CollisionStay = delegate {};
        public event Action<Collision> CollisionExit = delegate {};
        public event Action<Collider> TriggerEnter = delegate { };
        public event Action<Collider> TriggerStay = delegate { };
        public event Action<Collider> TriggerExit = delegate { };

        public void OnCollisionEnter(Collision collision) => CollisionEnter?.Invoke(collision);
        public void OnCollisionStay(Collision collision) => CollisionStay?.Invoke(collision);
        public void OnCollisionExit(Collision collision) => CollisionExit?.Invoke(collision);
        public void OnTriggerEnter(Collider other) => TriggerEnter?.Invoke(other);
        public void OnTriggerStay(Collider other) => TriggerStay?.Invoke(other);
        public void OnTriggerExit(Collider other) => TriggerExit?.Invoke(other);

        public Collider Collider { get; private set; }
        private void Awake() { Collider = GetComponent<Collider>(); }
    }
}
