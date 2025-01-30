using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO.Ports;
using System.Threading;
using System;

public class PortConnect : MonoBehaviour
{
    private static PortConnect instance;
    private SerialPort serialPort;
    private Thread readThread;
    private bool isRunning = false;
    public float speed = 0f;
    public static PortConnect pm;
    
    [SerializeField]
    private string portName = "COM16"; // 사용할 포트 이름
    [SerializeField]
    private int baudRate = 9600; // 보드레이트
    public float sendInterval = 0.1f; // 명령 전송 주기 (초 단위)
    private string command = "R"; // 전송할 명령어 기본값 (종단문자는 \r\n 추가)

    private void Awake()
    {
        if (pm != null && pm != this)
        {
            Destroy(gameObject); // 이미 존재하는 인스턴스가 있다면 새로운 객체 제거
            return;
        }

        pm = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        try
        {
            serialPort = new SerialPort(portName, baudRate);
            serialPort.Open();
            serialPort.ReadTimeout = 1000;
            Debug.Log("Serial Port Opened: " + portName);

            isRunning = true;
            readThread = new Thread(ReadSerialData);
            readThread.Start();

            InvokeRepeating(nameof(SendCommand), 0f, sendInterval);
        }
        catch (Exception e)
        {
            Debug.LogError("Failed to open serial port: " + e.Message);
        }
    }

    private void SendCommand()
    {
        if (serialPort != null && serialPort.IsOpen)
        {
            string fullCommand = command + "\r\n";
            serialPort.Write(fullCommand);
        }
    }

    private void ReadSerialData()
    {
        while (isRunning)
        {
            try
            {
                if (serialPort != null && serialPort.IsOpen)
                {
                    string incomingData = serialPort.ReadLine();
                    if (float.TryParse(incomingData, out float numericValue))
                    {
                        speed = (numericValue / 120) * (210 * (float)Math.PI) / 100;
                    }
                    else
                    {
                        Debug.LogWarning("Received non-numeric data: " + incomingData);
                        speed = numericValue;
                    }
                }
            }
            catch (TimeoutException)
            {
                // 시간 초과 예외 무시
            }
            catch (Exception e)
            {
                Debug.LogError("Error reading from serial port: " + e.Message);
            }
        }
    }

    private void OnApplicationQuit()
    {
        Cleanup();
    }

    private void Cleanup()
    {
        isRunning = false;
        if (readThread != null && readThread.IsAlive)
        {
            readThread.Join();
        }

        if (serialPort != null && serialPort.IsOpen)
        {
            serialPort.Close();
            Debug.Log("Serial Port Closed");
        }
    }
}
