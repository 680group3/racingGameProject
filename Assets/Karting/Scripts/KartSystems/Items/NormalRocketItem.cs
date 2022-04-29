using UnityEngine;

namespace KartGame.KartSystems.Items
{
    public class NormalRocketItem : Item
    {
        public override void activate(Racer racer)
        {
            // shoot the rocket
        }

        public override void pickup(Racer racer)
        {
            racer.normalRocketPickup();
        }
    }
}