using UnityEngine;
using Photon.Pun;

namespace KartGame.KartSystems.Items
{
    public class NormalRocketItem : Item
    {

        public override void activate(Racer racer)
        {
            GameObject prefab = Resources.Load("Rocket16_Green") as GameObject;
            PhotonNetwork.Instantiate(prefab.name, racer.getMrig().transform.position, Quaternion.identity, 0);
        }

        public override void pickup(Racer racer)
        {
            racer.normalRocketPickup();
        }
    }
}