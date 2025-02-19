using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class PlatformMaker : MonoBehaviour
{
    public GameObject platform;
    public GameObject endCorridor;
    public GameObject existingSectionParent; // 마지막에 생성할 EndCorridor 프리팹

    public int corridorNumber = 0; // 생성할 플랫폼의 수 (-1일 경우 무한 생성)
    public float platformWidth;
    [SerializeField] protected List<GameObject> _spawnedPlatforms; // 생성된 플랫폼 관리
    [SerializeField] protected Image _blackImage;
    [SerializeField] protected float _waterOutDuration = 5f;
    protected MovementRecorder mr;
    
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
    
    public void AddCorrior(int num)
    {
        if (num != -56)
        {
            while (_spawnedPlatforms.Count > 1)
            {
                GameObject platformToRemove = _spawnedPlatforms[1];
                _spawnedPlatforms.RemoveAt(1);
                Destroy(platformToRemove);
            }
        }

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
        float lastPlatformEndZ = lastPlatform.transform.position.z;

        int platformCount = (num < 0) ? 50 : num;
        platformCount = (num == -56) ? 1 : platformCount;

        // num 만큼 새로운 플랫폼을 한 번에 생성
        for (int i = 0; i < platformCount; i++)
        {
            // 새로운 플랫폼 생성
            GameObject newPlatform = Instantiate(platform, Vector3.zero, Quaternion.identity, existingSectionParent.transform);

            BoxCollider newPlatformCollider = newPlatform.GetComponentInChildren<BoxCollider>();

            if (newPlatformCollider == null)
            {
                Debug.LogError("새 플랫폼에 BoxCollider가 없습니다!");
                return;
            }

            // 새 플랫폼의 시작 위치 계산 (끝부분에 정확히 이어지게)
            float newPlatformOffset = newPlatformCollider.bounds.extents.z * 2;
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

            // 마지막 플랫폼의 끝 위치 계산 (z 좌표)
            lastPlatformEndZ = newPlatform.transform.position.z;
        }

        if (num >= 0)
        {
            // EndCorridor 추가
            GameObject newEndCorridor = Instantiate(endCorridor, Vector3.zero, Quaternion.identity,
                existingSectionParent.transform);

            BoxCollider endCorridorCollider = newEndCorridor.GetComponentInChildren<BoxCollider>();

            if (endCorridorCollider == null)
            {
                Debug.LogError("EndCorridor에 BoxCollider가 없습니다!");
                return;
            }

            // EndCorridor 위치 설정
            float endCorridorOffset = endCorridorCollider.bounds.extents.z * 2;
            ;
            float endCorridorStartZ = lastPlatformEndZ + endCorridorOffset;

            Vector3 endCorridorPosition = new Vector3(
                _spawnedPlatforms[_spawnedPlatforms.Count - 1].transform.position.x,
                _spawnedPlatforms[_spawnedPlatforms.Count - 1].transform.position.y,
                endCorridorStartZ
            );

            newEndCorridor.transform.position = endCorridorPosition;
            newEndCorridor.transform.localScale = new Vector3(platformWidth, platform.transform.localScale.y,
                platform.transform.localScale.z);

            _spawnedPlatforms.Add(newEndCorridor);
        }
    }
    
    protected void OnTriggerEnter(Collider other)
    {
        if (corridorNumber < 0 && other.gameObject.CompareTag("PlatformTrigger"))
        {
            AddCorrior(-56);
        }
        if (other.gameObject.CompareTag("WaterTrigger"))
        {
            StartCoroutine(WaterTrigger());
        }
        Destroy(other);
    }
    
    protected IEnumerator FadeInImage(float duration)
    {
        Color color = _blackImage.color;
        float startAlpha = 0f;
        float endAlpha = 1f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(startAlpha, endAlpha, elapsed / duration);
            _blackImage.color = new Color(color.r, color.g, color.b, alpha);
            yield return null;
        }

        // Ensure final value
        _blackImage.color = new Color(color.r, color.g, color.b, endAlpha);
    }

    public abstract IEnumerator WaterTrigger();
}
