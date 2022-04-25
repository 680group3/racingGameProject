using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class TextFaceCamera : MonoBehaviour
{
	
    public CinemachineVirtualCamera myCamera;
	
    // Start is called before the first frame update
    void Start()
    {

    }
	void LateUpdate () {
		transform.LookAt(transform.position + myCamera.transform.rotation * Vector3.forward, myCamera.transform.rotation * Vector3.up);
		//transform.rotation = Quaternion.LookRotation( transform.position - Camera.main.transform.position );
	}
}
