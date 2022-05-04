using UnityEditor;
using UnityEngine;
using KartGame.KartSystems;

namespace Assets.Karting.Scripts.KartSystems
{
    public class Rocket : MonoBehaviour
    {
        [SerializeField]
        private float speed = 55.0f;
        public Racer owner;

        void Update()
        {
            transform.Translate(Vector3.up * speed * Time.deltaTime);
        }
        private void OnTriggerEnter(Collider other)
        {
            Rigidbody rb = other.attachedRigidbody;
            if (rb != null && !rb.Equals(owner.getMrig()))
            {
                // make the other car spin out
                Debug.Log("explode");
                rb.AddExplosionForce(float.MaxValue, rb.transform.position, float.MaxValue);
            }
        }
    }
}
