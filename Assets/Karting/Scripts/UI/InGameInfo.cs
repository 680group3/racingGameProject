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
            Vector3 vel = VehCtrl.ourPlayer.GetComponent<Rigidbody>().velocity;
            float speed = Mathf.Sqrt(vel.x * vel.x + vel.z * vel.z);
            Speed.text = "Speed: " + string.Format($"{Mathf.FloorToInt(speed * 3.6f)} km/h");
        }
    }
}