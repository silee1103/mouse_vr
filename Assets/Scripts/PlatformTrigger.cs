using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlatformTrigger : MonoBehaviour
{
    public GameObject platform;
    public float platformWidth;
    public GameObject section;

    private List<GameObject> _spawnedPlatforms; // 생성된 플랫폼 관리

    private void Start()
    {
        platformWidth = 1;
        _spawnedPlatforms = new List<GameObject>();
        _spawnedPlatforms.Add(section);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("PlatformTrigger"))
        {
            GameObject go = Instantiate(platform, other.GetComponentInParent<Transform>().position + new Vector3(0,0, 24), Quaternion.identity);
            Vector3 currentScale = platform.transform.localScale;
            go.transform.localScale = new Vector3(platformWidth, currentScale.y, currentScale.z);
            
            // 생성된 플랫폼 관리 리스트에 추가
            _spawnedPlatforms.Add(go);
            
            other.enabled = false;
        }
    }
    
    public void OnSliderValueChanged(float value)
    {
        platformWidth = value;
        // 모든 생성된 플랫폼의 x local scale 업데이트
        foreach (GameObject platform in _spawnedPlatforms)
        {
            if (platform != null)
            {
                Vector3 currentScale = platform.transform.localScale;
                platform.transform.localScale = new Vector3(platformWidth, currentScale.y, currentScale.z);
            }
        }
    }
    
}
