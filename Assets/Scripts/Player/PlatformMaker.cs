using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// 플랫폼(발판) 생성 및 관리 클래스 (추상 클래스)
// - 특정 개수 또는 무한 생성 모드로 플랫폼을 생성
// - 슬라이더 값을 기반으로 플랫폼 너비 조정 가능
// - 플랫폼 끝에 EndCorridor(끝 발판) 추가
// - 특정 트리거(물과 충돌 등) 시 효과 발생
public abstract class PlatformMaker : MonoBehaviour
{
    [Header("Platform Settings")]
    public GameObject platform;           // 기본 플랫폼 프리팹
    public List<GameObject> corridorPrefabs;
    private int corridorType = 0;
    public GameObject endCorridor;        // 마지막에 배치할 EndCorridor 프리팹
    public GameObject existingSectionParent; // 생성된 플랫폼을 포함할 부모 오브젝트

    [Header("Generation Settings")]
    public int corridorNumber = 0;  // 생성할 플랫폼 개수 (-1일 경우 무한 생성)
    public float platformWidth;      // 플랫폼 너비

    [SerializeField] protected List<GameObject> _spawnedPlatforms; // 생성된 플랫폼 리스트

    [Header("UI & Effects")]
    [SerializeField] protected Image _blackImage;  // 페이드인 효과 이미지
    [SerializeField] protected float _waterOutDuration = 5f; // 물에 닿았을 때 페이드 지속 시간

    protected MovementRecorder mr; // 이동 데이터 기록기

    // 슬라이더 값 변경 시 호출 -> 모든 생성된 플랫폼의 너비 조정
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

    public void corridorPrefabsChangeBtn()
    {
        GameObject newObj;
        corridorType = (corridorType + 1) % corridorPrefabs.Count;
        // 모든 생성된 플랫폼의 x local scale 업데이트
        for (int i = 0; i < _spawnedPlatforms.Count - 1; i++)
        {
            GameObject platform = _spawnedPlatforms[i];
            if (platform != null)
            {
                newObj = ReplaceWithNewPrefab(platform, corridorPrefabs[corridorType]);
                _spawnedPlatforms[i] = newObj;
            }
        }

        newObj = ReplaceWithNewPrefab(_spawnedPlatforms[_spawnedPlatforms.Count - 1],endCorridor);
        _spawnedPlatforms[_spawnedPlatforms.Count-1] = newObj;

        platform = corridorPrefabs[corridorType];
    }
    
    public static GameObject ReplaceWithNewPrefab(GameObject original, GameObject newPrefab)
    {
        // 기존 정보 저장
        Transform t = original.transform;
        Vector3 pos = t.position;
        Quaternion rot = t.rotation;
        Vector3 scale = t.localScale;
        Transform parent = t.parent;

        // 기존 오브젝트 제거
        GameObject.DestroyImmediate(original); // Editor에서 실행할 때는 DestroyImmediate

        // 새 프리팹 인스턴스화
        GameObject newObj = GameObject.Instantiate(newPrefab, pos, rot, parent);
        newObj.transform.localScale = scale;

        return newObj;
    }

    // 새로운 플랫폼 추가 함수
    public void AddCorrior(int num)
    {
        if (num != -56)
        {
            // 기존 플랫폼 삭제 (맨 첫 번째 제외)
            while (_spawnedPlatforms.Count > 1)
            {
                GameObject platformToRemove = _spawnedPlatforms[1];
                _spawnedPlatforms.RemoveAt(1);
                Destroy(platformToRemove);
            }
        }

        // 마지막으로 추가된 플랫폼 참조
        GameObject lastPlatform = _spawnedPlatforms[_spawnedPlatforms.Count - 1];

        // 마지막 플랫폼의 BoxCollider 가져오기
        BoxCollider lastPlatformCollider = lastPlatform.GetComponentInChildren<BoxCollider>();
        if (lastPlatformCollider == null)
        {
            Debug.LogError("마지막 플랫폼에 BoxCollider가 없습니다!");
            return;
        }

        // 마지막 플랫폼의 끝 위치 계산 (Z축 기준)
        float lastPlatformEndZ = lastPlatform.transform.position.z;

        // 생성할 플랫폼 개수 설정 (-1일 경우 기본값 50)
        int platformCount = (num < 0) ? 50 : num;
        platformCount = (num == -56) ? 1 : platformCount;

        // num 만큼 새로운 플랫폼 생성
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

            // 마지막 플랫폼 위치 업데이트
            lastPlatformEndZ = newPlatform.transform.position.z;
        }

        if (num >= 0)
        {
            // EndCorridor 추가 (플랫폼 끝에 배치)
            GameObject newEndCorridor = Instantiate(endCorridor, Vector3.zero, Quaternion.identity, existingSectionParent.transform);

            BoxCollider endCorridorCollider = newEndCorridor.GetComponentInChildren<BoxCollider>();
            if (endCorridorCollider == null)
            {
                Debug.LogError("EndCorridor에 BoxCollider가 없습니다!");
                return;
            }

            // EndCorridor 위치 설정
            float endCorridorOffset = endCorridorCollider.bounds.extents.z * 2;
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

    // 트리거 충돌 이벤트 처리
    protected void OnTriggerEnter(Collider other)
    {
        // 무한 생성 모드 && PlatformTrigger와 충돌 시 새로운 플랫폼 생성
        if (corridorNumber < 0 && other.gameObject.CompareTag("PlatformTrigger"))
        {
            AddCorrior(-56);
        }
        
        // WaterTrigger와 충돌 시 페이드 효과 실행
        if (other.gameObject.CompareTag("WaterTrigger"))
        {
            StartCoroutine(WaterTrigger());
        }

        Destroy(other); // 충돌한 오브젝트 제거
    }

    // 페이드인 효과를 적용하는 코루틴
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

        // 마지막 알파값 설정 (완전히 검게 만듦)
        _blackImage.color = new Color(color.r, color.g, color.b, endAlpha);
    }

    // 물과 충돌했을 때 실행되는 추상 메서드 (상속받은 클래스에서 구현)
    public abstract IEnumerator WaterTrigger();
}
