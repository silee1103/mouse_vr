using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO.Ports;
using System.Threading;
using System.Text;
using UnityEngine.Profiling;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class PortConnect : MonoBehaviour
{
    // 싱글턴 인스턴스 (여러 씬에서 유지할 경우)
    public static PortConnect instance;

    [Header("Serial Port Settings")]
    [SerializeField] private string portName = "COM16";  // 환경에 맞게 변경
    [SerializeField] private int baudRate = 115200;        // Arduino와 동일

    // 디버그 로그 활성화 여부 (true이면 로그 출력)
    public bool DEBUG = true;
    public MovementRecorder mr;

    private SerialPort serialPort;
    private Thread readThread;
    private bool isRunning = false;

    // 스레드 안전 메시지 큐 (바이너리 메시지)
    private Queue<byte[]> messageQueue = new Queue<byte[]>();
    private readonly object queueLock = new object();

    // Arduino에서 받은 데이터 (바이너리 메시지)
    public uint startTime = 0;    // START 메시지의 startTime (4바이트)
    public uint elapsedTime = 0;  // DATA 메시지의 경과 시간 (4바이트, 마이크로초 단위)
    
    // y축 Δy 값 누적 (Arduino에서 전달받은 Δy 값: 부호 있는 정수)
    public int cumulativeDeltaY = 0;
    // 최근에 받은 Δy 값 (부호 있는 정수)
    public int lastDeltaY = 0;
    // 마지막 두 메시지 간 경과 시간 (초 단위)
    public float lastDeltaTimeSec = 0f;
    private uint previousElapsedTime = 0; 
    
    [Header("Position & Speed Settings")]
    // tick 당 이동 거리 (예: 0.1 cm)
    public float distancePerDelta = 0.1f;
    public float position = 0f;   // 누적 위치 (cm)
    public float speed = 0f;      // 평균 속도 (cm/s)

    // 디버그 로그 출력 간격 (초)
    private float lastLogTime = 0f;
    private float logInterval = 1f; // 1초에 한 번 로그 출력

    void Awake()
    {
        // 싱글턴 패턴 적용
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 새 씬이 로드될 때 MovementRecorder 찾기
        mr = FindObjectOfType<MovementRecorder>();

        if (mr == null)
        {
            Debug.LogWarning($"[DEBUG] No MovementRecorder found in scene '{scene.name}'");
        }
        else
        {
            Debug.Log($"[DEBUG] Found MovementRecorder in scene '{scene.name}'");
        }
    }

    void Start()
    {
        try
        {
            serialPort = new SerialPort(portName, baudRate);
            serialPort.ReadTimeout = 1000;
            serialPort.Open();
            if (DEBUG)
            {
                Debug.Log("[DEBUG] Serial Port Opened: " + portName);
            }
            isRunning = true;
            readThread = new Thread(ReadSerial);
            readThread.Start();
            
            SendStartCommand();
        }
        catch (Exception e)
        {
            Debug.LogError("[DEBUG] Failed to open serial port: " + e.Message);
        }
    }

    void FixedUpdate()
    {
        // 메시지 큐에 쌓인 데이터를 처리
        ProcessMessageQueue();

        // 누적 Δy 값을 cm 단위의 이동 거리로 환산
        position = cumulativeDeltaY * distancePerDelta;
        // 최근 Δy 값과 메시지 간 시간 차를 이용하여 순간 속도(cm/s) 계산
        if (lastDeltaTimeSec > 0)
        {
            speed = (lastDeltaY * distancePerDelta) / lastDeltaTimeSec * 0.0254f;
        }
        else
        {
            speed = 0f;
        }
        
        mr.Record();

        // 지정한 간격마다 디버그 로그 출력
        if (Time.time - lastLogTime >= logInterval)
        {
            lastLogTime = Time.time;
            if (DEBUG)
            {
                Debug.Log(
                    $"[DEBUG] Position: {position:F2} cm, Speed: {speed:F2} cm/s, Last Δy: {lastDeltaY}, Cumulative Δy: {cumulativeDeltaY}, Elapsed Time: {elapsedTime} ms");
            }
        }
    }

    void OnApplicationQuit()
    {
        isRunning = false;
        if (readThread != null && readThread.IsAlive)
        {
            readThread.Join();
        }
        if (serialPort != null && serialPort.IsOpen)
        {
            serialPort.Close();
        }
    }

    // 별도의 스레드에서 Arduino로부터 Serial 데이터를 읽어 내부 버퍼에 추가
    void ReadSerial()
    {
        List<byte> buffer = new List<byte>();
        while (isRunning)
        {
            try
            {
                int bytesToRead = serialPort.BytesToRead;
                if (bytesToRead > 0)
                {
                    byte[] temp = new byte[bytesToRead];
                    int bytesRead = serialPort.Read(temp, 0, bytesToRead);
                    buffer.AddRange(temp);

                    if (DEBUG)
                    {
                        Debug.Log($"[DEBUG] Received {bytesRead} bytes: {BitConverter.ToString(temp)}");
                    }

                    // 버퍼 내에서 0xAA 헤더를 기준으로 메시지 프레임을 정렬
                    while (buffer.Count >= 2)
                    {
                        int headerIndex = buffer.IndexOf(0xAA);
                        if (headerIndex < 0)
                        {
                            // 헤더가 없으면 버퍼를 모두 삭제
                            buffer.Clear();
                            break;
                        }
                        if (headerIndex > 0)
                        {
                            // 헤더 이전의 데이터는 버림
                            buffer.RemoveRange(0, headerIndex);
                        }
                        if (buffer.Count < 2)
                        {
                            break;  // 메시지 타입까지 수신하지 못한 경우 대기
                        }
                        byte msgType = buffer[1];
                        // 메시지 길이 결정 (0x02: START 메시지 = 6바이트, 0x01: DATA 메시지 = 10바이트)
                        int msgLength = (msgType == 0x02) ? 6 : (msgType == 0x01 ? 10 : -1);
                        if (msgLength == -1)
                        {
                            // 알 수 없는 메시지 타입이면 헤더 바이트 제거 후 재시도
                            buffer.RemoveAt(0);
                            continue;
                        }
                        if (buffer.Count < msgLength)
                        {
                            // 전체 메시지가 수신되지 않은 경우 대기
                            break;
                        }
                        // 완전한 메시지 추출
                        byte[] message = buffer.GetRange(0, msgLength).ToArray();
                        buffer.RemoveRange(0, msgLength);
                        lock (queueLock)
                        {
                            messageQueue.Enqueue(message);
                        }
                        if (DEBUG)
                        {
                            Debug.Log($"[DEBUG] Enqueued message of type {msgType} with length {msgLength}");
                        }
                    }
                }
            }
            catch (TimeoutException)
            {
                // ReadTimeout은 무시
            }
            catch (Exception ex)
            {
                Debug.LogError("[DEBUG] Serial read error: " + ex.Message);
            }
            Thread.Sleep(10);  // CPU 사용률 낮추기 위한 대기
        }
    }

    // 메시지 큐에 쌓인 바이너리 메시지를 처리
    void ProcessMessageQueue()
    {
        lock (queueLock)
        {
            while (messageQueue.Count > 0)
            {
                byte[] msg = messageQueue.Dequeue();
                ParseMessage(msg);
            }
        }
    }

    // 바이너리 메시지 파싱
    // START 메시지 (6바이트): [0xAA][0x02][startTime (4바이트)]
    // DATA 메시지 (10바이트): [0xAA][0x01][elapsedTime (4바이트)][encoderCount (4바이트)]
    void ParseMessage(byte[] msg)
    {
        if (msg.Length < 2)
            return;

        byte msgType = msg[1];
        if (msgType == 0x02 && msg.Length == 6)
        {
            startTime = BitConverter.ToUInt32(msg, 2);
            if (DEBUG)
            {
                Debug.Log("[DEBUG] START message received. Start time: " + startTime);
            }
        }
        else if (msgType == 0x01 && msg.Length == 10)
        {
            uint currentTime = BitConverter.ToUInt32(msg, 2);
            elapsedTime = currentTime;
            int deltaY = BitConverter.ToInt32(msg, 6); // y축 Δy 값 (부호 있음)
            if (previousElapsedTime != 0)
            {
                lastDeltaTimeSec = (currentTime - previousElapsedTime) / 1000f;
            }
            previousElapsedTime = currentTime;
            cumulativeDeltaY += deltaY;
            lastDeltaY = deltaY;
            if (DEBUG)
            {
                Debug.Log($"[DEBUG] DATA received. Elapsed: {elapsedTime} ms, Δy: {deltaY}");
            }
        }
        else
        {
            Debug.LogWarning($"[DEBUG] Unknown message received. Type: {msgType}, Length: {msg.Length}");
        }
    }

    // Arduino로 명령어 전송 (예: "RESET", "LICK")
    public void SendCommand(string cmd)
    {
        if (serialPort != null && serialPort.IsOpen)
        {
            string fullCmd = cmd + "\n";
            byte[] commandBytes = Encoding.ASCII.GetBytes(fullCmd);
            serialPort.Write(commandBytes, 0, commandBytes.Length);
            serialPort.BaseStream.Flush();
            if (DEBUG)
            {
                Debug.Log("[DEBUG] Sent command: " + cmd);
            }
        }
    }

    // 릭포트(보상) 실행 명령 ("1" 또는 "LICK")
    public void SendLickCommand()
    {
        SendCommand("L");
    }

    // 측정 리셋 명령 ("RESET")
    public void SendResetCommand()
    {
        SendCommand("R");
    }
    
    public void SendStartCommand()
    {
        SendCommand("S");
    }
    
    public void SendTriggerCommand()
    {
        SendCommand("T");
    }
    
    public void SendEndCommand()
    {
        SendCommand("E");
    }
}
