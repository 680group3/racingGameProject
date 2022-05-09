using UnityEngine;
using Photon.Pun;
using ExitGames.Client.Photon;

namespace KartGame.KartSystems.Items
{
    public class LiveRocketItem : Item
    {
        [SerializeField]
        private float speed = 55.0f;
        public int owner;

        public override void activate(Racer racer)
        {
            float magnitude = 20000.0f;
            racer.getMrig().AddRelativeForce(Vector3.up * magnitude, ForceMode.Impulse);
        }

        public override void pickup(Racer racer)
        {
            // do nothing; don't call this
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
                PhotonView pv = PhotonView.Get(racer);
                Debug.Log("Colliding with " + pv.ViewID);
                if (racer && pv.ViewID != owner)
                {
                    this.activate(racer);
                    this.gameObject.SetActive(false);
                }
            }
        }
    }
}