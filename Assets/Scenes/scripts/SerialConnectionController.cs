using UnityEngine;
using System.IO.Ports;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Unity.VisualScripting;

public class SerialConnectionController : MonoBehaviour
{
    public static SerialConnectionController Instance { get; private set; }

    SerialPort serialPort;
    [SerializeField] UIController ui_controller;
    string[] ports;
    [SerializeField] List<PartController> servos;
    public PartController s1, s2, s3, s4, s5, s6;

    // Debe coincidir EXACTO con SYNC_LOW / SYNC_HIGH del Arduino
    private const byte SYNC_LOW = 0x55;
    private const byte SYNC_HIGH = 0xAA;

    private Coroutine sendCoroutine;

    // --- Hilo de escritura (Unity solo envia, nunca lee del puerto) ---
    Thread writeThread;
    volatile bool writeRunning;

    // Ultimo mensaje calculado, pendiente de enviar por el hilo de escritura.
    readonly object lastSentLock = new object();
    byte[] lastSent = null;
    bool hasPending = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        GetPorts();
    }

    public void Connect(int port_index)
    {
        if (ports == null || ports.Length == 0)
        {
            Debug.LogError("No hay puertos disponibles (ports es null o vacio).");
            if (ui_controller != null) ui_controller.ButtonConnectFail();
            return;
        }

        if (port_index < 0 || port_index >= ports.Length)
        {
            Debug.LogError($"port_index ({port_index}) fuera de rango. ports.Length = {ports.Length}");
            if (ui_controller != null) ui_controller.ButtonConnectFail();
            return;
        }

        DisconnectPort();

        string connection_port = ports[port_index];
        serialPort = new SerialPort(connection_port, 115200)
        {
            WriteTimeout = 16
        };

        try
        {
            serialPort.Open();

            writeRunning = true;
            writeThread = new Thread(WriteLoop) { IsBackground = true };
            writeThread.Start();

            if (ui_controller != null) ui_controller.ButtonConnectSuccess();
            Debug.Log("Conectado al puerto correctamente");

            sendCoroutine = StartCoroutine(SendServosPeriodicallyCoroutine());
        }
        catch (Exception e)
        {
            Debug.LogException(e);
            DisconnectPort();
            if (ui_controller != null) ui_controller.ButtonConnectFail();
        }
    }

    void DisconnectPort()
    {
        if (sendCoroutine != null)
        {
            StopCoroutine(sendCoroutine);
            sendCoroutine = null;
        }

        writeRunning = false;
        if (writeThread != null && writeThread.IsAlive)
            writeThread.Join(200);
        writeThread = null;

        if (serialPort != null)
        {
            if (serialPort.IsOpen) serialPort.Close();
            serialPort.Dispose();
            serialPort = null;
        }
    }

    void OnDestroy() => DisconnectPort();
    void OnApplicationQuit() => DisconnectPort();

    public string[] GetPorts()
    {
        ports = SerialPort.GetPortNames();
        return ports;
    }

    IEnumerator SendServosPeriodicallyCoroutine()
    {
        while (serialPort != null && serialPort.IsOpen)
        {
            yield return new WaitForSeconds(0.01f);
            SendRobotAngles();
        }
    }

    // Corre en el hilo principal: SOLO calcula los bytes (necesita leer
    // los PartController) y los deja listos en 'pendingMess'. Nunca
    // llama a serialPort.Write directamente, para que el hilo de Unity
    // no pueda quedar esperando ese I/O bajo ninguna circunstancia.
    public void SendRobotAngles()
    {
        if (serialPort == null || !serialPort.IsOpen) return;

        PartController[] joints = { s1, s2, s3, s4, s5, s6 };
        byte[] mess = new byte[14];

        for (int i = 0; i < joints.Length; i++)
        {
            PartController s = joints[i];
            int val = MapServoAngle(s.currentAngle, s.minAngOut, s.maxAngOut, s.minValue, s.maxValue);
            byte[] b = DecimalToBytes(val);
            mess[i * 2] = b[0];
            mess[i * 2 + 1] = b[1];
        }

        // 7mo par reutilizado como marca de sincronizacion
        mess[12] = SYNC_LOW;
        mess[13] = SYNC_HIGH;

        lock (lastSentLock)
        {
            lastSent = mess;
            hasPending = true;
        }
    }

    // Corre en un hilo aparte. Nunca bloquea mas de WriteTimeout (16ms):
    // solo escribe el ultimo mensaje pendiente calculado por
    // SendRobotAngles. No lee nada del puerto - Unity solo envia.
    void WriteLoop()
    {
        while (writeRunning)
        {
            try
            {
                byte[] toWrite = null;
                lock (lastSentLock)
                {
                    if (hasPending)
                    {
                        toWrite = lastSent;
                        hasPending = false;
                    }
                }

                if (toWrite != null && serialPort != null && serialPort.IsOpen)
                {
                    serialPort.Write(toWrite, 0, toWrite.Length);
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning("Error al escribir en el puerto serie: " + e.Message);
            }

            Thread.Sleep(2);
        }
    }

    private int MapServoAngle(float angle, float minAngle, float maxAngle, float minValue, float maxValue)
    {
        angle = Mathf.Clamp(angle, minAngle, maxAngle);
        return (int)Mathf.Lerp(minValue, maxValue, (angle - minAngle) / (maxAngle - minAngle));
    }

    public byte[] DecimalToBytes(int decima)
    {
        return BitConverter.GetBytes((short)decima);
    }

    public void Home()
    {
        Debug.Log("should home");
    }
}