using System;
using UnityEngine;

namespace BlueMuffinGames.Utility
{
    [RequireComponent(typeof(Collider2D))]
    public class ColliderEvents2D : MonoBehaviour
    {
        public event Action<Collision2D> CollisionEnter = delegate {};
        public event Action<Collision2D> CollisionStay = delegate {};
        public event Action<Collision2D> CollisionExit = delegate {};
        public event Action<Collider2D> TriggerEnter = delegate { };
        public event Action<Collider2D> TriggerStay = delegate { };
        public event Action<Collider2D> TriggerExit = delegate { };

        public void OnCollisionEnter2D(Collision2D collision) => CollisionEnter?.Invoke(collision);
        public void OnCollisionStay2D(Collision2D collision) => CollisionStay?.Invoke(collision);
        public void OnCollisionExit2D(Collision2D collision) => CollisionExit?.Invoke(collision);
        public void OnTriggerEnter2D(Collider2D other) => TriggerEnter?.Invoke(other);
        public void OnTriggerStay2D(Collider2D other) => TriggerStay?.Invoke(other);
        public void OnTriggerExit2D(Collider2D other) => TriggerExit?.Invoke(other);

        public Collider2D Collider { get; private set; }
        private void Awake() { Collider = GetComponent<Collider2D>(); }
    }
}
