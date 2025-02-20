using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using TMPro;

// 객체의 이동 경로를 불러와 재생하고, LineRenderer를 통해 이동 경로를 시각화하는 클래스
// 재현 객체(e.g. Animal)에 할당 후 UI 생성하여 아래의 public 설명에 맞게 inspector에서 할당 
public class MovementLiner : MonoBehaviour
{
    [Header("UI Components")]
    public TMP_InputField inputField;   // 사용자 입력을 위한 InputField (파일 번호 입력)
    public TMP_Text currentTimeText;    // 현재 재생 시간 표시 텍스트

    [Header("Replay Settings")]
    public Transform objectToReplay;    // 이동을 재생할 대상 오브젝트
    public LineRenderer lineRendererPrefab; // 경로를 시각화할 LineRenderer 프리팹
    public Transform lineParent;        // LineRenderer들의 부모 오브젝트

    // 이동 데이터를 저장하는 리스트
    private List<Vector3> positions = new List<Vector3>(); // 위치 데이터
    private List<float> rotations = new List<float>();     // 회전 데이터 (Y축)
    private List<float> times = new List<float>();         // 타임스탬프 데이터

    private List<LineRenderer> lineRenderers = new List<LineRenderer>(); // 생성된 LineRenderer 목록

    private int currentStep = 0;   // 현재 재생 중인 스텝 (데이터 인덱스)
    private float replayStartTime = 0f; // 재생 시작 시간
    private float totalDuration = 0f;   // 전체 이동 시간

    void Start()
    {
        // loadButton.onClick.AddListener(LoadAndReplayPath); // 버튼 이벤트 리스너 (사용 시 주석 해제)
    }

    void Update()
    {
        if (positions.Count > 0 && currentStep < positions.Count - 1)
        {
            float elapsedTime = Time.time - replayStartTime; // 경과 시간 계산

            if (elapsedTime >= times[currentStep]) // 현재 스텝의 시간 도달 시
            {
                currentStep++; // 다음 스텝으로 이동

                // 위치 보간 (Lerp)
                objectToReplay.position = Vector3.Lerp(
                    positions[currentStep - 1],
                    positions[currentStep],
                    (elapsedTime - times[currentStep - 1]) / (times[currentStep] - times[currentStep - 1])
                );

                // 회전 보간 (Lerp)
                float interpolatedRotation = Mathf.Lerp(
                    rotations[currentStep - 1],
                    rotations[currentStep],
                    (elapsedTime - times[currentStep - 1]) / (times[currentStep] - times[currentStep - 1])
                );
                objectToReplay.rotation = Quaternion.Euler(0, interpolatedRotation, 0);
            }

            UpdateTimeDisplay(elapsedTime); // 현재 재생 시간 UI 업데이트
        }
    }

    // 이동 데이터를 로드하고 경로를 재생하는 함수
    public void LoadAndReplayPath()
    {
        string inputNumber = inputField.text;
        string path = Application.persistentDataPath + $"/MovementData{inputNumber}.txt";

        if (File.Exists(path))
        {
            // 기존 데이터 초기화
            positions.Clear();
            rotations.Clear();
            times.Clear();
            foreach (var lineRenderer in lineRenderers)
            {
                Destroy(lineRenderer.gameObject); // 기존 라인 삭제
            }
            lineRenderers.Clear();

            // 파일의 모든 줄 읽기
            string[] lines = File.ReadAllLines(path);

            foreach (string line in lines)
            {
                string[] parts = line.Split(',');

                // 올바른 형식의 데이터인지 확인 후 파싱
                if (parts.Length == 4 &&
                    float.TryParse(parts[0], out float time) &&
                    float.TryParse(parts[1], out float x) &&
                    float.TryParse(parts[2], out float z) &&
                    float.TryParse(parts[3], out float rotationY))
                {
                    positions.Add(new Vector3(x, objectToReplay.position.y, z));
                    rotations.Add(rotationY);
                    times.Add(time);
                }
            }

            DrawPath(); // 이동 경로 시각화

            // 재생 초기화
            currentStep = 0;
            replayStartTime = Time.time;
            if (positions.Count > 0)
            {
                objectToReplay.position = positions[0];
                objectToReplay.rotation = Quaternion.Euler(0, rotations[0], 0);
            }
            totalDuration = times[times.Count - 1] - times[0];
            UpdateTimeDisplay(0f);
        }
        else
        {
            Debug.LogError($"File not found at path: {path}");
        }
    }

    // 이동 경로를 시각적으로 표시하는 함수
    private void DrawPath()
    {
        for (int i = 0; i < positions.Count - 1; i++)
        {
            // 새 LineRenderer 생성
            LineRenderer segmentLine = Instantiate(lineRendererPrefab, lineParent);
            segmentLine.positionCount = 2;
            segmentLine.SetPosition(0, positions[i]);
            segmentLine.SetPosition(1, positions[i + 1]);

            // 색상 설정 (회전 값에 따라 색상 변화)
            float rotationY = rotations[i] % 360f;
            Color segmentColor = GetColorFromRotation(rotationY);
            segmentLine.startColor = segmentColor;
            segmentLine.endColor = segmentColor;

            segmentLine.widthMultiplier = 0.1f; // 라인 두께 설정

            lineRenderers.Add(segmentLine);
        }
    }

    // Y축 회전에 따른 색상 설정 함수
    private Color GetColorFromRotation(float rotationY)
    {
        if (rotationY >= 0 && rotationY < 90)
            return Color.Lerp(Color.red, Color.blue, rotationY / 90f); // 빨강 -> 파랑
        else if (rotationY >= 90 && rotationY < 180)
            return Color.Lerp(Color.blue, Color.yellow, (rotationY - 90) / 90f); // 파랑 -> 노랑
        else if (rotationY >= 180 && rotationY < 270)
            return Color.Lerp(Color.yellow, Color.green, (rotationY - 180) / 90f); // 노랑 -> 초록
        else
            return Color.Lerp(Color.green, Color.red, (rotationY - 270) / 90f); // 초록 -> 빨강
    }

    // 현재 재생 시간을 업데이트하는 함수
    private void UpdateTimeDisplay(float elapsedTime)
    {
        currentTimeText.text = $"{elapsedTime:F2}/{totalDuration:F2}s";
    }
}
