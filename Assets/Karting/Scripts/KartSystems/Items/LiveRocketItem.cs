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
        public double spawnTime;

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
             float dt = (float)( PhotonNetwork.Time - spawnTime );
            transform.Translate(Vector3.up * speed * Time.deltaTime);
        }

        public override void OnTriggerEnter(Collider other)
        {
            var rb = other.attachedRigidbody;
            if (rb)
            { // wild hacks
                var racer = rb.GetComponent<Racer>();
                PhotonView pv = PhotonView.Get(racer);
                if (!pv.IsMine) { return; }
                Debug.Log("Colliding with " + pv.ViewID);
                if (racer && pv.ViewID != owner)
                {
                    PhotonView photonView = other.gameObject.GetComponentInParent(typeof(PhotonView)) as PhotonView;
                    Racer r2 = photonView.gameObject.GetComponentInParent(typeof(Racer)) as Racer;
                 this.activate(r2);
                    this.gameObject.SetActive(false);
                    
                }
            }

        }

    }
}