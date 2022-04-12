using KartGame.KartSystems;
using UnityEngine;
using UnityEngine.Events;

public class ArcadeKartPowerup : MonoBehaviour {

    public Racer.StatPowerup boostStats = new Racer.StatPowerup
    {
        MaxTime = 5
    };

    public bool isCoolingDown { get; private set; }
    public float lastActivatedTimestamp { get; private set; }

    public float cooldown = 5f;

    public bool disableGameObjectWhenActivated;
    public UnityEvent onPowerupActivated;
    public UnityEvent onPowerupFinishCooldown;

    private void Awake()
    {
        lastActivatedTimestamp = -9999f;
    }


    private void Update()
    {
        if (isCoolingDown) { 

            if (Time.time - lastActivatedTimestamp > cooldown) {
                //finished cooldown!
                isCoolingDown = false;
                onPowerupFinishCooldown.Invoke();
            }

        }
    }


    private void OnTriggerEnter(Collider other)
    {
        if (isCoolingDown) return;

        var rb = other.attachedRigidbody;
        if (rb) {

            var racer = rb.GetComponent<Racer>();

            if (racer)
            { 
                lastActivatedTimestamp = Time.time;
                racer.speedBoost();
                onPowerupActivated.Invoke();
                isCoolingDown = true;

                if (disableGameObjectWhenActivated) this.gameObject.SetActive(false);
            }
        }
    }

}
