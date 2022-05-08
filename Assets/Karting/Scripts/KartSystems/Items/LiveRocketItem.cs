using UnityEngine;

namespace KartGame.KartSystems.Items
{
    public class LiveRocketItem : Item
    {
        [SerializeField]
        private float speed = 55.0f;
        public Racer owner;

        public override void activate(Racer racer)
        {
            float magnitude = 10.0f;
            // racer.getMrig().AddExplosionForce(50000000.0f, racer.transform.position, 50.0f);
            // racer.getMrig().AddRelativeTorque(Vector3.left * 9999999999.0f, ForceMode.Impulse);
            // racer.getMrig().AddRelativeForce(Vector3.forward * magnitude, ForceMode.Impulse);
            racer.getMrig().AddExplosionForce(magnitude, racer.getMrig().transform.position, magnitude, magnitude, ForceMode.Impulse);
        }

        public override void pickup(Racer racer)
        {
            racer.liveRocketPickup();
        }
        void Update()
        {
            transform.Translate(Vector3.up * speed * Time.deltaTime);
        }

        public override void OnTriggerEnter(Collider other)
        {
            var rb = other.attachedRigidbody;
            if (rb)
            {
                var racer = rb.GetComponent<Racer>();
                if (racer && !racer.Equals(owner))
                {
                    this.pickup(racer);
                    this.gameObject.SetActive(false);
                }
            }
        }
    }
}