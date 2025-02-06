using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System;
using System.IO.Ports;

public class PortConnect3D : PortConnect
{
    private bool isRunning = false;

    // 읽은 패킷을 저장할 큐 (스레드 안전)
    private Queue<SensorPacket> packetQueue = new Queue<SensorPacket>();
    private readonly object queueLock = new object();

    // 센서 데이터 구조 (타임스탬프, 센서1: dx,dy, 센서2: dx,dy)
    public struct SensorPacket
    {
        public uint time;
        public short sensor1_dx;
        public short sensor1_dy;
        public short sensor2_dx;
        public short sensor2_dy;
    }

    // 누적 움직임 (필요에 따라 스케일 조정)
    public Vector2 sensor1TotalMovement = Vector2.zero;
    public Vector2 sensor2TotalMovement = Vector2.zero;
    
    /// <summary>
    /// 별도의 스레드에서 SerialPort로부터 바이트 데이터를 읽고,
    /// 누적 버퍼에서 14바이트 패킷(헤더 0xAA, 푸터 0x55)을 추출하여 파싱한다.
    /// </summary>
    protected void ReadSerialData()
    {
        List<byte> buffer = new List<byte>();

        while (isRunning)
        {
            try
            {
                int b = serialPortMain.ReadByte();
                if (b == -1) continue;
                buffer.Add((byte)b);

                // 버퍼에 14바이트 이상 있으면 패킷을 파싱 시도
                while (buffer.Count >= 14)
                {
                    // 패킷 헤더 체크 (0xAA)
                    if (buffer[0] != 0xAA)
                    {
                        buffer.RemoveAt(0);
                        continue;
                    }
                    // 14번째 바이트 (인덱스 13)가 푸터(0x55)인지 확인
                    if (buffer[13] != 0x55)
                    {
                        // 푸터가 올바르지 않으면 헤더를 버리고 다시 시도
                        buffer.RemoveAt(0);
                        continue;
                    }

                    // 유효한 패킷 발견: 14바이트 추출
                    byte[] packetBytes = buffer.GetRange(0, 14).ToArray();
                    buffer.RemoveRange(0, 14);

                    // 파싱 (little endian)
                    uint timestamp = (uint)(packetBytes[1] 
                                            | (packetBytes[2] << 8) 
                                            | (packetBytes[3] << 16) 
                                            | (packetBytes[4] << 24));
                    short s1_dx = (short)(packetBytes[5] | (packetBytes[6] << 8));
                    short s1_dy = (short)(packetBytes[7] | (packetBytes[8] << 8));
                    short s2_dx = (short)(packetBytes[9] | (packetBytes[10] << 8));
                    short s2_dy = (short)(packetBytes[11] | (packetBytes[12] << 8));

                    SensorPacket packet = new SensorPacket
                    {
                        time = timestamp,
                        sensor1_dx = s1_dx,
                        sensor1_dy = s1_dy,
                        sensor2_dx = s2_dx,
                        sensor2_dy = s2_dy
                    };

                    lock (queueLock)
                    {
                        packetQueue.Enqueue(packet);
                    }
                }
            }
            catch (TimeoutException)
            {
                // 타임아웃은 무시
            }
            catch (Exception e)
            {
                Debug.LogError("Serial read error: " + e.Message);
            }
        }
    }


    private void Update()
    {
        // 큐에 쌓인 패킷들을 처리
        while (true)
        {
            SensorPacket packet;
            lock (queueLock)
            {
                if (packetQueue.Count > 0)
                    packet = packetQueue.Dequeue();
                else
                    break;
            }

            // 여기서는 예시로 각 센서의 델타값을 누적하여 움직임을 계산
            sensor1TotalMovement.x += packet.sensor1_dx;
            sensor1TotalMovement.y += packet.sensor1_dy;
            sensor2TotalMovement.x += packet.sensor2_dx;
            sensor2TotalMovement.y += packet.sensor2_dy;

            // 디버깅: 패킷 내용을 로그로 출력할 수도 있음.
            // Debug.Log($"Time: {packet.time} | S1: ({packet.sensor1_dx},{packet.sensor1_dy}) | S2: ({packet.sensor2_dx},{packet.sensor2_dy})");
        }

        // 예시: 센서1의 누적 움직임을 오브젝트의 위치로 반영 (스케일 조정)
        transform.position = new Vector3(sensor1TotalMovement.x * 0.01f, sensor1TotalMovement.y * 0.01f, 0);
    }
}
