using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using TMPro;

public class MovementLiner : MonoBehaviour
{
    public TMP_InputField inputField;
    public Button loadButton;
    public Transform objectToReplay;
    public LineRenderer lineRendererPrefab; // LineRenderer 프리팹
    public Transform lineParent;
    public TMP_Text currentTimeText;

    private List<Vector3> positions = new List<Vector3>();
    private List<float> rotations = new List<float>();
    private List<float> times = new List<float>();
    private List<LineRenderer> lineRenderers = new List<LineRenderer>();

    private int currentStep = 0;
    private float replayStartTime = 0f;
    private float totalDuration = 0f;

    void Start()
    {
        // loadButton.onClick.AddListener(LoadAndReplayPath);
    }

    void Update()
    {
        if (positions.Count > 0 && currentStep < positions.Count - 1)
        {
            float elapsedTime = Time.time - replayStartTime;

            if (elapsedTime >= times[currentStep])
            {
                currentStep++;
                objectToReplay.position = Vector3.Lerp(
                    positions[currentStep - 1],
                    positions[currentStep],
                    (elapsedTime - times[currentStep - 1]) / (times[currentStep] - times[currentStep - 1])
                );

                float interpolatedRotation = Mathf.Lerp(
                    rotations[currentStep - 1],
                    rotations[currentStep],
                    (elapsedTime - times[currentStep - 1]) / (times[currentStep] - times[currentStep - 1])
                );
                objectToReplay.rotation = Quaternion.Euler(0, interpolatedRotation, 0);
            }
            UpdateTimeDisplay(elapsedTime);
        }
    }

    public void LoadAndReplayPath()
    {
        string inputNumber = inputField.text;
        string path = Application.persistentDataPath + $"/MovementData{inputNumber}.txt";

        if (File.Exists(path))
        {
            positions.Clear();
            rotations.Clear();
            times.Clear();
            foreach (var lineRenderer in lineRenderers)
            {
                Destroy(lineRenderer.gameObject); // 기존 라인 삭제
            }
            lineRenderers.Clear();

            string[] lines = File.ReadAllLines(path);

            foreach (string line in lines)
            {
                string[] parts = line.Split(',');
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

            DrawPath();

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

    private void DrawPath()
    {
        for (int i = 0; i < positions.Count - 1; i++)
        {
            // 새 LineRenderer 생성
            LineRenderer segmentLine = Instantiate(lineRendererPrefab, lineParent);
            segmentLine.positionCount = 2;
            segmentLine.SetPosition(0, positions[i]);
            segmentLine.SetPosition(1, positions[i + 1]);

            // 색상 설정
            float rotationY = rotations[i] % 360f;
            Color segmentColor = GetColorFromRotation(rotationY);
            segmentLine.startColor = segmentColor;
            segmentLine.endColor = segmentColor;

            segmentLine.widthMultiplier = 0.1f;

            lineRenderers.Add(segmentLine);
        }
    }

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

    private void UpdateTimeDisplay(float elapsedTime)
    {
        currentTimeText.text = $"{elapsedTime:F2}/{totalDuration:F2}s";
    }
}
