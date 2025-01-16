using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformTrigger : MonoBehaviour
{
    public GameObject platform;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("PlatformTrigger"))
        {
            Instantiate(platform, other.GetComponentInParent<Transform>().position + new Vector3(0,0, 24), Quaternion.identity);
            other.enabled = false;
        }
    }
}
