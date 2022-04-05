using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using KartGame.KartSystems;
    
public class VehCtrl : MonoBehaviour {
	public List<AxleInfo> axleInfos;
	public float maxMotorTorque;
	public float maxSteeringAngle;
	public float brakeTorque;
	public float decelerationForce;
	private Rigidbody mrig;
	private Racer racer;
	
	float eTorque;
	float steering;
	
    public float AntiRoll = 5000.0f;
	
	public void ApplyLocalPositionToVisuals (AxleInfo axleInfo)
	{
		Vector3 position;
		Quaternion rotation;
		axleInfo.leftWheelCollider.GetWorldPose (out position, out rotation);
		axleInfo.leftWheelMesh.transform.position = position;
		axleInfo.leftWheelMesh.transform.rotation = rotation;
		axleInfo.rightWheelCollider.GetWorldPose (out position, out rotation);
		axleInfo.rightWheelMesh.transform.position = position;
		axleInfo.rightWheelMesh.transform.rotation = rotation;
	}

	void Awake ()
	{
		racer = GetComponent<Racer>();
		mrig = GetComponent<Rigidbody>();
		mrig.centerOfMass = new Vector3(0, 0.3f, 0);
	}

	void FixedUpdate ()
	{
		if (!racer.GetCanMove()) {
			return;
		}
		
        //racer.ActivateDriftVFX(true);
		
		eTorque = maxMotorTorque * Input.GetAxis ("Vertical");
		steering = maxSteeringAngle * Input.GetAxis ("Horizontal");
		for (int i = 0; i < axleInfos.Count; i++)
		{
			AxleInfo ax = axleInfos[i];
			WheelCollider WheelL = ax.leftWheelCollider;
			WheelCollider WheelR = ax.rightWheelCollider;
			
			racer.ActivateDriftVFX(false);
			// determine when tire marks should appear based on wheel slip
			WheelHit hitL, hitR;
			if (WheelL.GetGroundHit (out hitL) && WheelR.GetGroundHit (out hitR)) {
				if ((hitL.forwardSlip < -0.5 || hitR.forwardSlip > 0.5) || hitL.sidewaysSlip > 0.5 || hitR.sidewaysSlip > 0.5) {
		           // Debug.Log ("WHEEL SLIPPING");
					racer.ActivateDriftVFX(true);
				}
			} 
			
			if (ax.steering)
			{
				Steering (ax, steering);
			}
			if (ax.motor)
			{
				Acceleration (ax, eTorque);
			}
			if (Input.GetKey (KeyCode.Space))
			{
				Brake(ax);					
			} 
			if (Input.GetKey(KeyCode.R)) {
		        transform.rotation = Quaternion.Euler(0, 0, 0);
				mrig.velocity = Vector3.zero;
				mrig.angularVelocity = Vector3.zero;
		      //  transform.Translate(0, 1, 0);
			}
	
			// stabilization bar simulation ala
			// https://forum.unity.com/threads/how-to-make-a-physically-real-stable-car-with-wheelcolliders.50643/
			WheelHit hit;
		    float travelL = 1.0f;
		    float travelR = 1.0f;
     
		    var groundedL = WheelL.GetGroundHit(out hit);
		   	if (groundedL)
				travelL = (-WheelL.transform.InverseTransformPoint(hit.point).y - WheelL.radius) / WheelL.suspensionDistance;
     
			var groundedR = WheelR.GetGroundHit(out hit);
			if (groundedR)
				travelR = (-WheelR.transform.InverseTransformPoint(hit.point).y - WheelR.radius) / WheelR.suspensionDistance;
     
			var antiRollForce = (travelL - travelR) * AntiRoll;
     
			if (groundedL)
				mrig.AddForceAtPosition(WheelL.transform.up * -antiRollForce, WheelL.transform.position);  
			if (groundedR)
				mrig.AddForceAtPosition(WheelR.transform.up * antiRollForce, WheelR.transform.position);  
						
			ApplyLocalPositionToVisuals (ax);
		}
	}

	private void Acceleration (AxleInfo axleInfo, float motor)
	{
		if (motor != 0f)
		{
			axleInfo.leftWheelCollider.brakeTorque = 0;
			axleInfo.rightWheelCollider.brakeTorque = 0;
			axleInfo.leftWheelCollider.motorTorque = motor;
			axleInfo.rightWheelCollider.motorTorque = motor;
		} else
		{
			Deceleration (axleInfo);
		}
	}

	private void Deceleration (AxleInfo axleInfo)
	{
		axleInfo.leftWheelCollider.brakeTorque = decelerationForce;
		axleInfo.rightWheelCollider.brakeTorque = decelerationForce;
	}

	private void Steering (AxleInfo axleInfo, float steering)
	{
		axleInfo.leftWheelCollider.steerAngle = steering;
		axleInfo.rightWheelCollider.steerAngle = steering;
	}

	private void Brake (AxleInfo axleInfo)
	{
		axleInfo.leftWheelCollider.brakeTorque = brakeTorque;
		axleInfo.rightWheelCollider.brakeTorque = brakeTorque;
	}
}
    
[System.Serializable]
public class AxleInfo {
    public WheelCollider leftWheelCollider;
    public WheelCollider rightWheelCollider;
	public GameObject leftWheelMesh;
	public GameObject rightWheelMesh;
    public bool motor; // is this wheel attached to motor?
    public bool steering; // does this wheel apply steer angle?
}
