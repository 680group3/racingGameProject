using UnityEditor;
using UnityEngine;

namespace Assets.Karting.Scripts.KartSystems
{
    public class Rocket : MonoBehaviour
    {
        [SerializeField]
        private float speed = 55.0f;

        void Update()
        {
            transform.Translate(Vector3.up * speed * Time.deltaTime);
        }
    }
}