using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalCamera : MonoBehaviour {

	public Transform m_otherCamera;
	public Transform m_otherPortal;

    // Update is called once per frame
    void LateUpdate()
    {
        Vector3 userOffsetFromPortal = Camera.main.transform.position - transform.position;
        m_otherCamera.transform.position = m_otherPortal.transform.position + userOffsetFromPortal;

        m_otherCamera.transform.rotation = m_otherPortal.rotation * Quaternion.Inverse(transform.rotation) * Camera.main.transform.rotation;
    }

}
