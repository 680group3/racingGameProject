using KartGame.KartSystems;
using TMPro;
using UnityEngine;

namespace KartGame.UI
{
    public class InGameInfo : MonoBehaviour
    {
        public TextMeshProUGUI Speed;

        void Start()
        {
           
        }

        // Update is called once per frame
        void Update()
        {
            float speed = VehCtrl.ourPlayer.GetComponent<Rigidbody>().velocity.magnitude;
            Speed.text = "Speed: " + string.Format($"{Mathf.FloorToInt(speed * 3.6f)} km/h");
        }
    }
}