using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlatformTriggerTut : PlatformMaker
{
    public GameObject platform;
    public GameObject endCorridor; // 마지막에 생성할 EndCorridor 프리팹
    public GameObject existingSectionParent;
    [SerializeField] private Image _blackImage;
    [SerializeField] private float _waterOutDuration = 5f;

    private void Start()
    {
        platformWidth = 1;
        _spawnedPlatforms = new List<GameObject>();
        corridorNumber = StatusManager.sm.GetTutNum();
        
        // 기존 섹션의 자식들을 리스트로 추가
        if (existingSectionParent != null)
        {
            // 부모의 자식 객체들을 가져와 리스트에 추가
            foreach (Transform child in existingSectionParent.transform)
            {
                _spawnedPlatforms.Add(child.gameObject);
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

            // corridorNumber 만큼 새로운 플랫폼을 한 번에 생성
            for (int i = 0; i < corridorNumber; i++)
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
                Debug.Log(lastPlatformEndZ +" + "+newPlatformOffset+" = "+newPlatformStartZ);

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

            // EndCorridor 추가
            GameObject newEndCorridor = Instantiate(endCorridor, Vector3.zero, Quaternion.identity, existingSectionParent.transform);
            
            BoxCollider endCorridorCollider = newEndCorridor.GetComponentInChildren<BoxCollider>();

            if (endCorridorCollider == null)
            {
                Debug.LogError("EndCorridor에 BoxCollider가 없습니다!");
                return;
            }

            // EndCorridor 위치 설정
            float endCorridorOffset = endCorridorCollider.bounds.extents.z * 2;;
            float endCorridorStartZ = lastPlatformEndZ + endCorridorOffset;

            Vector3 endCorridorPosition = new Vector3(
                _spawnedPlatforms[_spawnedPlatforms.Count - 1].transform.position.x,
                _spawnedPlatforms[_spawnedPlatforms.Count - 1].transform.position.y,
                endCorridorStartZ
            );

            newEndCorridor.transform.position = endCorridorPosition;
            newEndCorridor.transform.localScale = new Vector3(platformWidth, platform.transform.localScale.y, platform.transform.localScale.z);

            _spawnedPlatforms.Add(newEndCorridor);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("WaterTrigger"))
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
        if (StatusManager.sm.IsTutLeft()){
            StatusManager.sm.IncreaseTutStage();
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
        else
        {
            SceneManager.LoadScene("MouseTrainScene_Corridor");
        }
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
