using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using TMPro;

public class MovementLiner : MonoBehaviour
{
    public TMP_InputField inputField; // 숫자를 입력받는 InputField
    public Button loadButton; // 로드 버튼
    public Transform objectToReplay; // 움직임을 재현할 오브젝트
    public LineRenderer lineRenderer; // 경로를 그릴 LineRenderer
    public TMP_Text currentTimeText; // 현재 시간을 표시하는 TMP 텍스트
    
    private List<Vector3> positions = new List<Vector3>(); // 파일에서 읽은 위치 데이터
    private List<float> times = new List<float>(); // 파일에서 읽은 시간 데이터

    private int currentStep = 0; // 재현 진행 상태
    private float replayStartTime = 0f; // 재현 시작 시간
    private float totalDuration = 0f; // 전체 경로의 총 시간


    void Start()
    {
        loadButton.onClick.AddListener(LoadAndReplayPath);
        lineRenderer.enabled = false;
    }

    void Update()
    {
        // 움직임 재현
        if (positions.Count > 0 && currentStep < positions.Count - 1)
        {
            // 현재 시간 계산
            float elapsedTime = Time.time - replayStartTime;

            // 다음 위치로 보간 이동
            if (elapsedTime >= times[currentStep])
            {
                currentStep++;
                objectToReplay.position = Vector3.Lerp(
                    positions[currentStep - 1],
                    positions[currentStep],
                    (elapsedTime - times[currentStep - 1]) / (times[currentStep] - times[currentStep - 1])
                );
            }
            UpdateTimeDisplay(elapsedTime);
        }
    }

    public void LoadAndReplayPath()
    {
        // 입력된 숫자를 사용해 파일 경로 생성
        string inputNumber = inputField.text;
        string path = Application.persistentDataPath + $"/MovementData{inputNumber}.txt";

        if (File.Exists(path))
        {
            // 파일에서 데이터 읽기
            positions.Clear();
            times.Clear();
            string[] lines = File.ReadAllLines(path);

            foreach (string line in lines)
            {
                string[] parts = line.Split(',');
                if (parts.Length == 3 && 
                    float.TryParse(parts[0], out float time) &&
                    float.TryParse(parts[1], out float x) &&
                    float.TryParse(parts[2], out float z))
                {
                    positions.Add(new Vector3(x, objectToReplay.position.y, z));
                    times.Add(time);
                }
            }

            // 경로 표시
            DrawPath();
            
            Debug.Log("Positions Count: " + positions.Count);
            foreach (var pos in positions)
            {
                Debug.Log($"Position: {pos}");
            }

            Debug.Log($"LineRenderer Position Count: {lineRenderer.positionCount}");


            // 재현 준비
            currentStep = 0;
            replayStartTime = Time.time;
            if (positions.Count > 0)
            {
                objectToReplay.position = positions[0];
            }
            // 전체 경로 시간 계산
            totalDuration = times[times.Count - 1] - times[0];
            UpdateTimeDisplay(0f);
        }
        else
        {
            Debug.LogError($"File not found at path: {path}");
        }
    }

    private void DrawPath()
    {
        // LineRenderer로 경로 표시
        lineRenderer.enabled = true;
        lineRenderer.positionCount = positions.Count;
        lineRenderer.SetPositions(positions.ToArray());
        lineRenderer.startColor = Color.red;
        lineRenderer.endColor = Color.red;
        lineRenderer.widthMultiplier = 0.1f;
    }
    
    private void UpdateTimeDisplay(float elapsedTime)
    {
        // TMP 텍스트 업데이트
        currentTimeText.text = $"{elapsedTime:F2}/{totalDuration:F2}s";
    }

}
