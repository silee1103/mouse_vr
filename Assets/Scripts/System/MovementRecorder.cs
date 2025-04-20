using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using Vector3 = UnityEngine.Vector3;

// 캐릭터의 이동 데이터를 기록하고, 주기적으로 파일로 저장하는 클래스
// - Circular Buffer를 사용하여 이동 데이터 저장
// - 일정 주기로 데이터를 파일에 비동기 저장
// - 씬 변경 및 종료 시 남은 데이터 저장
public class MovementRecorder : MonoBehaviour
{
    [Header("Recording Settings")]
    public int bufferSize = 500;  // Circular Buffer 크기 (저장할 최대 데이터 수)
    public float saveInterval = 5f; // 데이터를 저장하는 간격 (초 단위)

    // 이동 데이터 저장 버퍼
    private Queue<(Vector3 position, float rotation, float time, float speed)> movementBuffer; // Circular Buffer
    private Queue<(Vector3 position, float rotation, float time, float speed)> savingBuffer;  // Saving Buffer (파일 저장용)

    private float saveTimer = 0f; // 저장 타이머
    private string dirPath; // 데이터 저장 경로

    private void Awake()
    {
        // 씬이 언로드될 때 데이터 저장
        SceneManager.sceneUnloaded += OnSceneUnload;
    }

    private void Start()
    {
        // 프레임 속도 제한 설정 (40 FPS)
        // Application.targetFrameRate = 40;

        // 버퍼 초기화
        movementBuffer = new Queue<(Vector3, float, float, float)>(bufferSize);
        savingBuffer = new Queue<(Vector3, float, float, float)>(bufferSize);
        dirPath = Application.persistentDataPath; // 파일 저장 경로
    }

    private void Update()
    {
        // 주기적으로 데이터 저장
        saveTimer += Time.deltaTime;
        if (saveTimer >= saveInterval)
        {
            saveTimer = 0f;
            SaveBufferAsync(); // 데이터를 비동기 저장
        }
    }

    // 현재 캐릭터의 위치, 회전, 시간, 속도를 기록
    public void Record()
    {
        RecordPosition(transform.position, transform.rotation.eulerAngles.y, PortConnect.instance.elapsedTime / 1000f, PortConnect.instance.speedY);
    }

    // 특정 이벤트(예: Lick 발생) 시 데이터 기록
    public void RecordLick()
    {
        RecordPosition(Vector3.zero, 0, -1, 0); // 특수 값 (-1)으로 기록
    }

    // 이동 데이터를 버퍼에 추가하는 함수
    private void RecordPosition(Vector3 position, float rotation, float time, float speed)
    {
        lock (movementBuffer)
        {
            if (movementBuffer.Count >= bufferSize)
            {
                movementBuffer.Dequeue(); // 오래된 데이터 제거
            }
            movementBuffer.Enqueue((position, rotation, time, speed)); // 새로운 데이터 추가
        }
    }

    // 비동기 방식으로 이동 데이터를 파일에 저장하는 함수
    private async void SaveBufferAsync()
    {
        // movementBuffer 데이터를 savingBuffer로 이동 (데이터 보호를 위해 lock 사용)
        lock (movementBuffer)
        {
            savingBuffer = new Queue<(Vector3, float, float, float)>(movementBuffer);
            movementBuffer.Clear(); // 기존 버퍼 초기화
        }

        string path = dirPath + $"/MovementData{StatusManager.instance.TXTRANDOM}.txt";

        // 비동기로 파일 저장 (메인 스레드 블로킹 방지)
        await Task.Run(() =>
        {
            using (StreamWriter writer = new StreamWriter(path, true))
            {
                while (savingBuffer.Count > 0)
                {
                    var data = savingBuffer.Dequeue();
                    writer.WriteLine($"{data.time},{data.position.x * 11.11},{data.position.z * 11.11},{data.rotation},{data.speed}");
                }
            }
            savingBuffer.Clear();
        });

        Debug.Log("Movement data saved in " + path);
    }

    // 남아 있는 이동 데이터를 즉시 저장하는 함수
    public void SaveRemainingBuffer()
    {
        string path = dirPath + $"/MovementData{StatusManager.instance.TXTRANDOM}.txt";

        // SavingBuffer에 남아 있는 데이터 저장
        lock (savingBuffer)
        {
            if (savingBuffer.Count > 0)
            {
                using (StreamWriter writer = new StreamWriter(path, true))
                {
                    while (savingBuffer.Count > 0)
                    {
                        var data = savingBuffer.Dequeue();
                        writer.WriteLine($"{data.time},{data.position.x * 11.11},{data.position.z * 11.11},{data.rotation},{data.speed * 1.08f / 12f}");
                    }
                }
                Debug.Log("Remaining movement data saved in " + path);
            }
        }

        // MovementBuffer에 남아 있는 데이터 저장
        lock (movementBuffer)
        {
            if (movementBuffer.Count > 0)
            {
                using (StreamWriter writer = new StreamWriter(path, true))
                {
                    while (movementBuffer.Count > 0)
                    {
                        var data = movementBuffer.Dequeue();
                        writer.WriteLine($"{data.time},{data.position.x * 11.11},{data.position.z * 11.11},{data.rotation},{data.speed}");
                    }
                }
                Debug.Log("Remaining movement data saved in " + path);
            }
        }
    }

    // 애플리케이션 종료 시 남은 데이터를 저장
    private void OnApplicationQuit()
    {
        Debug.Log("Application is quitting. Saving remaining buffer...");
        SaveRemainingBuffer();
    }

    // 씬이 언로드될 때 남은 데이터를 저장
    private void OnSceneUnload(Scene scene)
    {
        Debug.Log("Scene is changed. Saving remaining buffer...");
        SaveRemainingBuffer();
    }
}
