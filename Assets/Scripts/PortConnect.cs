using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Threading;
using System.Text;
using Unity.Mathematics;
using UnityEngine.SceneManagement;

// 아두이노와 연결되어 speed를 주기적으로 계산하고 / 아두이노 lick, reset, start, trigger, end 신호를 보낼 수 있는 함수
// 속도(cm/s): PortConnect.instance.speed
// 신호:
    // PortConnect.instance.SendLickCommand()
    // PortConnect.instance.SendResetCommand()
    // PortConnect.instance.SendStartCommand()
    // PortConnect.instance.SendTriggerCommand()
    // PortConnect.instance.SendEndCommand()
public class PortConnect : MonoBehaviour
{
    // singleton instance (모든 코드에서 PortConnect.instance.(public 변수나 함수 이름) 으로 접근 가능 + 그리고 unity가 꺼질 때까지 해당 스크립트를 가진 instance는 단 하나만 존재함)
    public static PortConnect instance;

    public bool is1D = false;

    [Header("Serial Port Settings")]
    [SerializeField] private string portName = "COM16";  // 환경에 맞게 변경
    [SerializeField] private int baudRate = 115200;        // Arduino와 동일
    private SerialPort serialPort;
    private Thread readThread;

    [Header("DEBUG (LOG) Settings")]
    [SerializeField] private bool DEBUG = true;
    private float lastLogTime = 0f;
    private float logInterval = 1f; // 1초에 한 번 로그 출력
    
    // 스레드 안전 메시지 큐 (바이너리 메시지)
    private Queue<byte[]> messageQueue = new Queue<byte[]>();
    private readonly object queueLock = new object();

    // Arduino에서 받은 데이터 (바이너리 메시지)
    public uint startTime = 0;    // START 메시지의 startTime (4바이트)
    public uint elapsedTime = 0;  // DATA 메시지의 경과 시간 (4바이트, 마이크로초 단위)
    
    public int cumulativeDeltaY = 0;    // y축 Δy 값 누적 (Arduino에서 전달받은 Δy 값: 부호 있는 정수)
    public int cumulativeDeltaX = 0;    // x축 Δx 값 누적 (Arduino에서 전달받은 Δx 값: 부호 있는 정수)
    
    public int lastDeltaY = 0;              // 최근에 받은 Δy 값 (부호 있는 정수)
    public int lastDeltaX = 0;              // 최근에 받은 Δx 값 (부호 있는 정수)

    public float lastDeltaTimeSec = 0f;     // 마지막 두 메시지 간 경과 시간 (초 단위)
    private uint previousElapsedTime = 0;   // 마지막 메세지 받은 시간
    
    [Header("Position & Speed Settings")]
    public float distancePerDelta = 0.1f;   // tick 당 이동 거리 (예: 0.1 cm)
    public float positionY = 0f;   // 누적 위치 (cm)
    public float speedY = 0f;      // 평균 속도 (cm/s)  !!!! 외부에서 사용 !!!
    
    public float positionX = 0f;   // 누적 위치 (cm)
    public float speedX = 0f;      // 평균 속도 (cm/s)  !!!! 외부에서 사용 !!!

    public float headRotation = 0f;
    private float hr = 0;
    
    // 추가 변수
    private MovementRecorder mr;    // Record 함수 호출 위함 (speed 계산 시에 Record)
    private bool isRunning = false; // 시작 시에 true, unity 종료 시에 false
    
    void Awake()
    {
        // Singleton pattern 적용
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
        
        // Scene load 시에 불릴 함수 추가
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    // Scene Load 시 마다 movement recorder를 찾아 mr 변수에 할당하도록 설정
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

    // 게임 시작 시에 serial 통신 port를 열고 serialPort에 endpoint 할당 + 통신용 thread 열어 ReadSerial 함수를 반복적으로 호출하도록 함
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

    // 특정 시간 때마다 ReadSerial 함수를 통해 쌓인 데이터를 speed로 변환
    // (Fixed Update는 default로 0.02초 주기 - Edit -> Project Settings -> Time에서 설정 가능)
    void FixedUpdate()
    {
        // readThread에서 메시지 큐에 쌓은 데이터를 처리
        ProcessMessageQueue();

        // 누적 Δy 값을 cm 단위의 이동 거리로 환산 (정확한 position이 필요하다면 MovementRecorder를 수정하여 해당 값 저장하도록 변경, 지금은 쓰이지 않음)
        positionY = cumulativeDeltaY * distancePerDelta;
        if (!is1D) { positionX = cumulativeDeltaX * distancePerDelta; }
        
        // 최근 Δy 값과 메시지 간 시간 차를 이용하여 순간 속도(cm/s) 계산
        if (lastDeltaTimeSec > 0)
        {
            headRotation = (hr * distancePerDelta) / lastDeltaTimeSec * 2* math.PI / 360;
            speedY = -(lastDeltaY * distancePerDelta) / lastDeltaTimeSec * 0.0254f; // 마우스는 1/100 인치 단위 사용 == [0.01인치=0.0254cm]
            if (!is1D) {speedX = -(lastDeltaX * distancePerDelta) / lastDeltaTimeSec * 0.0254f;}
        }
        else
        {
            headRotation = 0f;
            speedY = 0f;
            if (!is1D) {speedX = 0f;}
        }
        
        mr.Record(); // 파일에 Record하라는 신호

        // 지정한 간격마다 디버그 로그 출력
        if (Time.time - lastLogTime >= logInterval)
        {
            lastLogTime = Time.time;
            if (DEBUG)
            {
                if (is1D)
                {
                    Debug.Log(
                        $"[DEBUG] Position: {positionY:F2} cm, Speed: {speedY:F2} cm/s, Last Δy: {lastDeltaY}, Cumulative Δy: {cumulativeDeltaY}, Elapsed Time: {elapsedTime} ms");
                }
                else
                {
                    Debug.Log(
                        $"[DEBUG] Position X: {positionX:F2} cm, Position Y: {positionY:F2} cm\n" +
                        $"Speed X: {speedX:F2} cm/s, Speed Y: {speedY:F2} cm/s\n" +
                        $"Last Δx: {lastDeltaX}, Cumulative Δx: {cumulativeDeltaX}" +
                        $"Last Δy: {lastDeltaY}, Cumulative Δy: {cumulativeDeltaY}\n Elapsed Time: {elapsedTime} ms");
                }
            }
        }
    }

    // 앱 종효 시 Thread, serialPort 종료
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
                        int msgLength = (msgType == 0x02) ? 6 : (msgType == 0x01 ? 12 : -1);
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

    // 바이너리 메시지 파싱해서 변수에 할당
    // 바이너리 메세지 프로토콜
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
        else if (msgType == 0x01 && msg.Length == 12)
        {
            uint currentTime = BitConverter.ToUInt32(msg, 2);
            elapsedTime = currentTime;
            if (is1D)
            {
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
            } else
            {
                int deltaY = BitConverter.ToInt16(msg, 6); // y축 Δy 값 (부호 있음)
                int deltaX = BitConverter.ToInt16(msg, 8);
                int deltaRotation = BitConverter.ToInt16(msg, 10);  // Degrees
                
                if (previousElapsedTime != 0)
                {
                    lastDeltaTimeSec = (currentTime - previousElapsedTime) / 1000f;
                }

                previousElapsedTime = currentTime;
                cumulativeDeltaY += deltaY;
                cumulativeDeltaX += deltaX;
                lastDeltaY = deltaY;
                lastDeltaX = deltaX;
                hr = deltaRotation;
                
                if (DEBUG)
                {
                    Debug.Log($"[DEBUG] DATA received. Elapsed: {elapsedTime} ms, Δy: {deltaY}, Δx: {deltaX}");
                }
            }
        }
        else
        {
            Debug.LogWarning($"[DEBUG] Unknown message received. Type: {msgType}, Length: {msg.Length}");
        }
    }

    // Arduino로 명령어 전송 (예: "RESET", "LICK")
    void SendCommand(string cmd)
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

    
    // !!!! 외부에서 사용 !!!
    
    // 릭포트(보상) 실행 명령
    public void SendLickCommand()
    {
        SendCommand("L"); // 2D: 보상 신호; 1초간 PIN_LICK ON
    }
    // 측정 리셋 명령
    public void SendResetCommand()
    {
        SendCommand("R"); // 2D: (누적 이동량 및 타이머 초기화)
    }
    // 아두이노 보드 로직 시작 명령
    public void SendStartCommand()
    {
        SendCommand("S");
    }
    // inscopics 측정 시작 명령
    public void SendTriggerCommand()
    {
        SendCommand("T"); // 2D: 트리거 시작 (PIN_TRIGGER HIGH; 종료 명령 전까지 유지)
    }
    // inscopics 측정 종료 명령
    public void SendEndCommand()
    {
        SendCommand("E"); // 2D: 트리거 종료 (PIN_TRIGGER LOW)
    }
}
