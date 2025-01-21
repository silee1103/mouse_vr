using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO.Ports;
using System.Threading;
using System;

public class PortConnect : MonoBehaviour
{
    private SerialPort serialPort;
    private Thread readThread;
    private bool isRunning = false;
    public float speed = 0f;

    [SerializeField]
    private string portName = "COM16"; // 사용할 포트 이름
    [SerializeField]
    private int baudRate = 9600; // 보드레이트
    // [SerializeField]
    public float sendInterval = 0.1f; // 명령 전송 주기 (초 단위)

    private string command = "R"; // 전송할 명령어 기본값 (종단문자는 \r\n 추가)

    void Start()
    {
        // SerialPort 초기화
        try
        {
            serialPort = new SerialPort(portName, baudRate);
            serialPort.Open();
            serialPort.ReadTimeout = 1000; // 읽기 시간 초과 설정 (1초)
            Debug.Log("Serial Port Opened: " + portName);

            // 데이터 수신 스레드 시작
            isRunning = true;
            readThread = new Thread(ReadSerialData);
            readThread.Start();

            // 명령 전송 시작
            InvokeRepeating(nameof(SendCommand), 0f, sendInterval);
        }
        catch (Exception e)
        {
            Debug.LogError("Failed to open serial port: " + e.Message);
        }
    }

    private void SendCommand()
    {
        // 명령어 전송 (종단문자로 \r\n 포함)
        if (serialPort != null && serialPort.IsOpen)
        {
            string fullCommand = command + "\r\n";
            serialPort.Write(fullCommand); // Write 사용으로 \r\n 포함
            // Debug.Log("Sent: " + fullCommand);
        }
    }

    private void ReadSerialData()
    {
        // 데이터를 지속적으로 읽음
        while (isRunning)
        {
            try
            {
                if (serialPort != null && serialPort.IsOpen)
                {
                    string incomingData = serialPort.ReadLine(); // 한 줄 읽기
                    if (float.TryParse(incomingData, out float numericValue))
                    {
                        // Debug.Log("Received (float): " + numericValue);
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
                // 시간 초과 예외는 무시
            }
            catch (Exception e)
            {
                Debug.LogError("Error reading from serial port: " + e.Message);
            }
        }
    }
    

    private void OnApplicationQuit()
    {
        // 애플리케이션 종료 시 정리
        isRunning = false;
        if (readThread != null && readThread.IsAlive)
        {
            readThread.Join(); // 스레드 종료 대기
        }

        if (serialPort != null && serialPort.IsOpen)
        {
            serialPort.Close();
            Debug.Log("Serial Port Closed");
        }
    }
}
