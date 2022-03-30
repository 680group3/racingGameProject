using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WheelSetup : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        WheelCollider c = GetComponent<WheelCollider>();
		c.mass = 50;
		c.forceAppPointDistance = 0.2f;
		//c.wheelDampingRate = 0.1f;
		var ss = c.suspensionSpring;
		ss.spring = 28500;
		c.suspensionSpring = ss;
		var ff = c.forwardFriction;
		//ff.extremumSlip = 0.9f;
		c.forwardFriction = ff;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
