using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformMaker : MonoBehaviour
{
    public int corridorNumber = 0; // 생성할 플랫폼의 수 (-1일 경우 무한 생성)
    public float platformWidth;
    [SerializeField] protected List<GameObject> _spawnedPlatforms; // 생성된 플랫폼 관리
    
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
