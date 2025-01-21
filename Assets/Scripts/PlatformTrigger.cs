using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlatformTrigger : MonoBehaviour
{
    public GameObject platform;
    public float platformWidth;
    public GameObject existingSectionParent;

    private List<GameObject> _spawnedPlatforms; // 생성된 플랫폼 관리

    private void Start()
    {
        platformWidth = 1;
        _spawnedPlatforms = new List<GameObject>();
        
        // 기존 섹션의 자식들을 리스트로 추가
        if (existingSectionParent != null)
        {
            // 부모의 자식 객체들을 가져와 리스트에 추가
            foreach (Transform child in existingSectionParent.transform)
            {
                _spawnedPlatforms.Add(child.gameObject);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("PlatformTrigger"))
        {
            // 마지막으로 추가된 플랫폼 참조
            GameObject lastPlatform = _spawnedPlatforms[_spawnedPlatforms.Count - 1];

            // 마지막 플랫폼의 자식에서 BoxCollider 가져오기
            BoxCollider lastPlatformCollider = lastPlatform.GetComponentInChildren<BoxCollider>();

            if (lastPlatformCollider == null)
            {
                Debug.LogError("마지막 플랫폼에 BoxCollider가 없습니다!");
                return;
            }

            // 마지막 플랫폼의 끝 위치 계산 (z 좌표)
            float lastPlatformEndZ = lastPlatformCollider.bounds.max.z;

            // 새로운 플랫폼 생성
            GameObject newPlatform = Instantiate(platform, Vector3.zero, Quaternion.identity, existingSectionParent.transform);
            
            // 새로 생성될 플랫폼의 BoxCollider 가져오기
            BoxCollider newPlatformCollider = newPlatform.GetComponentInChildren<BoxCollider>();

            if (newPlatformCollider == null)
            {
                Debug.LogError("새 플랫폼에 BoxCollider가 없습니다!");
                return;
            }

            // 새 플랫폼의 시작 위치 계산 (끝부분에 정확히 이어지게)
            float newPlatformOffset = newPlatformCollider.bounds.max.z; // 시작 부분의 로컬 오프셋
            float newPlatformStartZ = lastPlatformEndZ + newPlatformOffset;

            Vector3 spawnPosition = new Vector3(
                lastPlatform.transform.position.x,
                lastPlatform.transform.position.y,
                newPlatformStartZ
            );

            newPlatform.transform.position = spawnPosition;

            // 새로운 플랫폼 크기 설정
            newPlatform.transform.localScale = new Vector3(platformWidth, platform.transform.localScale.y, platform.transform.localScale.z);

            // 생성된 플랫폼 리스트에 추가
            _spawnedPlatforms.Add(newPlatform);

            // 트리거 비활성화
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
