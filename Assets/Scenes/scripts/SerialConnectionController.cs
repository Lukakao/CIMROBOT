using UnityEngine;
using System.IO.Ports;
using System;
using System.Collections;
using System.Collections.Generic;
public class SerialConnectionController : MonoBehaviour
{
    public static SerialConnectionController Instance { get; private set; }

    SerialPort serialPort;
    [SerializeField] UIController ui_controller;
    string[] ports;
    [SerializeField] List<PartController> servos;
    
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
        string connection_port = ports[port_index];
        StartCoroutine(SendServosPeriodicallyCoroutine());
        serialPort = new SerialPort(connection_port, 9600);
        try
        {
            serialPort.Open();
            ui_controller.ButtonConnectSuccess();
            Debug.Log("Conectado al puerto correctamente");
        }
        catch
        {
            ui_controller.ButtonConnectFail();
            Debug.LogError("Fallo en conectar al puerto");
        }
    }

    public string[] GetPorts()
    {
        ports = SerialPort.GetPortNames();
        return ports;
    }
    IEnumerator SendServosPeriodicallyCoroutine()
    {
        while(true){ //serialPort.IsOpen
            yield return new WaitForSeconds(0.5f);
            SendRobotAngles();
        }
    }

    public void SendRobotAngles(){
        byte[] mess = new byte[14]; //6 joints 2bytes por joint faltan 2 bytes extra porque el riffo lo hizo asi
        int index = 0;
        int mappedValue = 0;
        
        foreach (var s in servos)
        {
            mappedValue = MapServoAngle(s.currentAngle,s.minAng,s.maxAng,s.minValue,s.maxValue);
            byte[] b = DecimalToBytes(mappedValue);
            mess[index] = b[0];
            mess[index+1] = b[1];
            index += 2;
        }
        
        
        mess[12] = DecimalToBytes(0)[0];
        mess[13] = DecimalToBytes(0)[1];
        try
        {
            serialPort.Write(mess, 0, mess.Length);
        }
        catch{}
        Debug.Log(mappedValue + " enviado: " + BitConverter.ToString(mess));
    }

    private int MapServoAngle(float angle, float minAngle, float maxAngle, float minValue, float maxValue)
    {
        angle = Mathf.Clamp(angle, minAngle, maxAngle);
        int mappedValue = (int)Mathf.Lerp(minValue, maxValue, (angle - minAngle) / (maxAngle - minAngle));
        return mappedValue;
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
