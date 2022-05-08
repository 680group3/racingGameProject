using UnityEngine;
using Photon.Pun;
using Assets.Karting.Scripts.KartSystems;

namespace KartGame.KartSystems.Items
{
    public class PickupRocketItem : Item
    {
        public override void activate(Racer racer)
        {
            GameObject prefab = Resources.Load("Rocket16_Green") as GameObject;
            Rigidbody mrig = racer.getMrig();
            Quaternion rotation = mrig.transform.rotation * Quaternion.Euler(90, 0, 0);
            GameObject rocket = PhotonNetwork.Instantiate(prefab.name, mrig.transform.position, rotation, 0);
            rocket.GetComponent<LiveRocketItem>().owner = racer;
        }

        public override void pickup(Racer racer)
        {
            racer.pickupRocketPickup();
        }
    }
}