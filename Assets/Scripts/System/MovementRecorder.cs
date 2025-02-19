using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;
using System.Numerics;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;
using Vector3 = UnityEngine.Vector3;

public class MovementRecorder : MonoBehaviour
{
    // public Transform characterTransform; // 캐릭터 Transform
    public int bufferSize = 500; // Circular Buffer 크기
    public float saveInterval = 5f; // 데이터를 저장하는 간격 (초 단위)

    private Queue<(Vector3 position, float rotation, float time, float speed)> movementBuffer; // Circular Buffer
    private Queue<(Vector3 position, float rotation, float time, float speed)> savingBuffer;  // Saving Buffer
    private float saveTimer = 0f; // 저장 타이머
    private string dirPath;
    // private int randomExpNum;

    private void Awake()
    {
        SceneManager.sceneUnloaded += OnSceneUnload;
    }

    
    private void Start()
    {
        Application.targetFrameRate = 40;
        movementBuffer = new Queue<(Vector3, float, float, float)>(bufferSize);
        savingBuffer = new Queue<(Vector3, float, float, float)>(bufferSize);
        dirPath = Application.persistentDataPath;
        // randomExpNum = PortConnect.instance.TXTRANDOM;
    }

    private void Update()
    {
        // 저장 타이머 업데이트
        saveTimer += Time.deltaTime;
        if (saveTimer >= saveInterval)
        {
            saveTimer = 0f;
            SaveBufferAsync(); // 데이터를 비동기로 저장
        }
    }

    public void Record()
    {
        RecordPosition(transform.position, transform.rotation.eulerAngles.y, PortConnect.instance.elapsedTime/1000f, PortConnect.instance.speed); // TODO? : /100f
    }

    public void RecordLick()
    {
        RecordPosition(Vector3.zero, 0, -1, 0);
    }

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

    private async void SaveBufferAsync()
    {
        // movementBuffer 데이터를 savingBuffer로 이동
        lock (movementBuffer)
        {
            savingBuffer = new Queue<(Vector3, float, float, float)>(movementBuffer);
            movementBuffer.Clear();
        }
        
        string path = dirPath + $"/MovementData{StatusManager.instance.TXTRANDOM}.txt";

        // 비동기로 파일 저장
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

        Debug.Log("Movement data saved in "+ path);
    }
    
    public void SaveRemainingBuffer()
    {
        string path = dirPath + $"/MovementData{StatusManager.instance.TXTRANDOM}.txt";
        
        
        lock (savingBuffer)
        {
            if (savingBuffer.Count > 0)
            {
                using (StreamWriter writer = new StreamWriter(path, true))
                {
                    while (savingBuffer.Count > 0)
                    {
                        var data = savingBuffer.Dequeue();
                        writer.WriteLine($"{data.time},{data.position.x * 11.11},{data.position.z * 11.11},{data.rotation},{data.speed * 1.08f/12f}");
                    }
                }
                Debug.Log("Remaining movement data saved in " + path);
            }
        }

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
    
    private void OnApplicationQuit()
    {
        Debug.Log("Application is quitting. Saving remaining buffer...");
        SaveRemainingBuffer();
    }

    private void OnSceneUnload(Scene scene)
    {
        Debug.Log("Scene is changed. Saving remaining buffer...");
        SaveRemainingBuffer();
    }
}
