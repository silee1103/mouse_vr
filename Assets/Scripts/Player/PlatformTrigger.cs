using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlatformTrigger : PlatformMaker
{
    public int maxCorridor = 17;
    public int minCorridor = 0;

    [SerializeField] private Image _blackImage;
    [SerializeField] private float _waterOutDuration = 5f;
    // private List<GameObject> _spawnedPlatforms; // 생성된 플랫폼 관리
    private int _currentCorridorCount = 0; // 현재 생성된 플랫폼 개수
    private bool _isCorridorEnded = false;

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

        corridorNumber = Random.Range(minCorridor, maxCorridor);
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

            if (corridorNumber == -1 || _currentCorridorCount < corridorNumber)
            {
                // 새로운 플랫폼 생성
                GameObject newPlatform = Instantiate(platform, Vector3.zero, Quaternion.identity,
                    existingSectionParent.transform);

                BoxCollider newPlatformCollider = newPlatform.GetComponentInChildren<BoxCollider>();

                if (newPlatformCollider == null)
                {
                    Debug.LogError("새 플랫폼에 BoxCollider가 없습니다!");
                    return;
                }

                // 새 플랫폼의 시작 위치 계산 (끝부분에 정확히 이어지게)
                float newPlatformOffset = newPlatformCollider.bounds.max.z;
                float newPlatformStartZ = lastPlatformEndZ + newPlatformOffset;

                Vector3 spawnPosition = new Vector3(
                    lastPlatform.transform.position.x,
                    lastPlatform.transform.position.y,
                    newPlatformStartZ
                );

                newPlatform.transform.position = spawnPosition;

                // 새로운 플랫폼 크기 설정
                newPlatform.transform.localScale = new Vector3(platformWidth, platform.transform.localScale.y,
                    platform.transform.localScale.z);

                // 생성된 플랫폼 리스트에 추가
                _spawnedPlatforms.Add(newPlatform);

                // 플랫폼 생성 개수 증가
                _currentCorridorCount++;
            }
            // corridorNumber가 -1이 아니고 플랫폼을 모두 생성한 경우
            else if (!_isCorridorEnded && _currentCorridorCount == corridorNumber)
            {
                // EndCorridor 생성
                GameObject newEndCorridor = Instantiate(endCorridor, Vector3.zero, Quaternion.identity,
                    existingSectionParent.transform);

                BoxCollider newPlatformCollider = newEndCorridor.GetComponentInChildren<BoxCollider>();

                if (newPlatformCollider == null)
                {
                    Debug.LogError("새 플랫폼에 BoxCollider가 없습니다!");
                    return;
                }

                // 새 플랫폼의 시작 위치 계산 (끝부분에 정확히 이어지게)
                float newPlatformOffset = newPlatformCollider.bounds.max.z;
                float newPlatformStartZ = lastPlatformEndZ + newPlatformOffset;
                
                Vector3 endCorridorPosition = new Vector3(
                    lastPlatform.transform.position.x,
                    lastPlatform.transform.position.y,
                    newPlatformStartZ
                );

                newEndCorridor.transform.position = endCorridorPosition;

                // 새로운 플랫폼 크기 설정
                newEndCorridor.transform.localScale = new Vector3(platformWidth, platform.transform.localScale.y,
                    platform.transform.localScale.z);

                _spawnedPlatforms.Add(newEndCorridor);

                // EndCorridor 생성 후 트리거 비활성화
                _isCorridorEnded = true;
            }
        }
        else if (other.gameObject.CompareTag("WaterTrigger"))
        {
            StartCoroutine(WaterTrigger());
        }
        Destroy(other);
    }

    private IEnumerator WaterTrigger()
    {
        yield return StartCoroutine(FadeInImage(1f));
        PortConnect.pm.SendWaterSign();
        yield return new WaitForSeconds(_waterOutDuration);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    
    IEnumerator FadeInImage(float duration)
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
    
}
