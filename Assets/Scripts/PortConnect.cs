using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO.Ports;
using System.Threading;
using System;

public class PortConnect : MonoBehaviour
{
    private static PortConnect instance;
    private SerialPort serialPortMain;
    private SerialPort serialPortUSB;
    private Thread readThreadMain;
    private Thread readThreadUSB;
    private bool isRunning = false;
    public float speed = 0f;
    public static PortConnect pm;
    
    [SerializeField]
    private string portNameMain = "COM16"; // 사용할 포트 이름
    [SerializeField]
    private string portNameUSB = "COM6"; // 사용할 포트 이름
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
            serialPortMain = new SerialPort(portNameMain, baudRate);
            serialPortMain.Open();
            serialPortMain.ReadTimeout = 1000;
            Debug.Log("Serial Port Opened: " + portNameMain);
            
            serialPortUSB = new SerialPort(portNameUSB, baudRate);
            serialPortUSB.Open();
            serialPortUSB.ReadTimeout = 1000;
            Debug.Log("Serial Port Opened: " + portNameUSB);

            isRunning = true;
            readThreadMain = new Thread(ReadSerialData);
            readThreadMain.Start();

            InvokeRepeating(nameof(SendCommand), 0f, sendInterval);
        }
        catch (Exception e)
        {
            Debug.LogError("Failed to open serial port: " + e.Message);
        }
    }

    public void SendWaterSign()
    {
        if (serialPortUSB != null && serialPortUSB.IsOpen)
        {
            serialPortUSB.Write("1");
            Debug.Log("Sent Water Signal to COM6");
        }
        else
        {
            Debug.LogError("COM6 not available");
        }
    }


    private void SendCommand()
    {
        if (serialPortMain != null && serialPortMain.IsOpen)
        {
            string fullCommand = command + "\r\n";
            serialPortMain.Write(fullCommand);
        }
    }

    private void ReadSerialData()
    {
        while (isRunning)
        {
            try
            {
                if (serialPortMain != null && serialPortMain.IsOpen)
                {
                    string incomingData = serialPortMain.ReadLine();
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
        if (readThreadMain != null && readThreadMain.IsAlive)
        {
            readThreadMain.Join();
        }
        
        if (readThreadUSB != null && readThreadUSB.IsAlive)
        {
            readThreadUSB.Join();
        }

        if (serialPortMain != null && serialPortMain.IsOpen)
        {
            serialPortMain.Close();
            Debug.Log("Serial Port Closed");
        }
        
        if (serialPortUSB != null && serialPortUSB.IsOpen)
        {
            serialPortUSB.Close();
            Debug.Log("Serial Port Closed");
        }
    }
}
