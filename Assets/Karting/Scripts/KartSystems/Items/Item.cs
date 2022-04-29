using UnityEngine;

namespace KartGame.KartSystems.Items
{
    public abstract class Item : MonoBehaviour
    {
        public GameObject item;
        public abstract void activate(Racer racer);

        public abstract void pickup(Racer racer);

        private void OnTriggerEnter(Collider other)
        {
            var rb = other.attachedRigidbody;
            if (rb)
            {
                var racer = rb.GetComponent<Racer>();
                if (racer && racer.getItem() == null)
                {
                    this.pickup(racer);
                    this.gameObject.SetActive(false);
                }
            }
        }
    }
}
