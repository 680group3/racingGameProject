using UnityEngine;

namespace KartGame.KartSystems.Items
{
    public abstract class Item : MonoBehaviour
    {
        public abstract void activate(Racer racer);
    }
}
