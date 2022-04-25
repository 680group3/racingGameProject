using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class TextFaceCamera : MonoBehaviour
{
	
    private CinemachineVirtualCamera vCam;
	
    // Start is called before the first frame update
    void Start()
    {
		if (vCam == null) {
			vCam = FindObjectOfType<CinemachineVirtualCamera>();
		}
    }
	void LateUpdate () {
		transform.LookAt(transform.position + vCam.transform.rotation * Vector3.forward, vCam.transform.rotation * Vector3.up);
		//transform.rotation = Quaternion.LookRotation( transform.position - Camera.main.transform.position );
	}
}
