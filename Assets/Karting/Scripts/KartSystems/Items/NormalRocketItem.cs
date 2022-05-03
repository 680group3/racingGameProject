using UnityEngine;

namespace KartGame.KartSystems.Items
{
    public class NormalRocketItem : Item
    {
        public override void activate(Racer racer)
        {
            GameObject prefab = Resources.Load<GameObject>("Assets/AurynSky/Rockets Missiles and Bombs/Prefabs/Green/Rocket16_Green.prefab");
            Instantiate(prefab, racer.getMrig().transform.position, Quaternion.identity);
        }

        public override void pickup(Racer racer)
        {
            racer.normalRocketPickup();
        }
    }
}