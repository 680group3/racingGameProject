using UnityEngine;

namespace KartGame.KartSystems.Items
{
    public class SpeedItem : Item
    {
        [Header("Speed Boost")]
        [Tooltip("The strength of the impulse force applied when activating a speed boost.")]
        public int boostStrength = 50000;

        public override void activate(Racer racer)
        {
            racer.getMrig().AddRelativeForce(Vector3.forward * this.boostStrength, ForceMode.Impulse);
        }
    }
}