using System;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    private bool isConfigVisible = false;
    private GameObject config_canvas;
    private TMP_Dropdown dropdown_puertos;
    public GameObject input_prefab;
    public GameObject axis_text_prefab;

    SerialConnectionController serial;
    private Button btn_conectar;
    private int selectedPortIndex;

    void Start()
    {
        config_canvas = GameObject.FindGameObjectWithTag("config_canvas");
        serial = GameObject.FindGameObjectWithTag("serial").GetComponent<SerialConnectionController>();
        dropdown_puertos = GameObject.FindGameObjectWithTag("dropdown_puertos").GetComponent<TMP_Dropdown>();
        dropdown_puertos.onValueChanged.AddListener(DropdownValueChanged);
        
        if (config_canvas)
        {
            Debug.Log("canvas found");
        }
        
        btn_conectar = GameObject.FindGameObjectWithTag("btn_conectar").GetComponent<Button>();
        btn_conectar.onClick.AddListener(TryConnectPort);

        config_canvas.SetActive(isConfigVisible);

        //GenerateTextInputsLimits();
        GenerarDropdown();

    }

    public void ButtonConnectSuccess()
    {
        ColorBlock cb = btn_conectar.colors;
        cb.normalColor = new Color(0.5f,1f,.5f);
        cb.selectedColor = new Color(0.5f,1f,.5f);
        btn_conectar.colors = cb;

    }
    public void ButtonConnectFail()
    {
        ColorBlock cb = btn_conectar.colors;
        cb.normalColor = new Color(1f,0.5f,0.5f);
        cb.selectedColor = new Color(1f,0.5f,0.5f);
        btn_conectar.colors = cb;

    }

    private void DropdownValueChanged(int value)
    {
        selectedPortIndex = value;
    }

    private void TryConnectPort()
    {
        serial.Connect(selectedPortIndex);
    }

    public void ToggleConfig()
    {
        if (config_canvas != null)
        {
            isConfigVisible = !isConfigVisible;
            config_canvas.SetActive(isConfigVisible);
        }
    }

    public void SetRobotInitialPosition()
    {
        //get all axis
        //save them in a json
        //update limits -> initial + limit ang (outdated)
    }

    public void GenerarDropdown()
    {
        List<TMP_Dropdown.OptionData> options = new List<TMP_Dropdown.OptionData>();
        string[] ports = serial.GetPorts();
        for (int i = 0; i < ports.Length; i++)
        {
            options.Add(new TMP_Dropdown.OptionData(ports[i]));
        }
        dropdown_puertos.ClearOptions();
        dropdown_puertos.AddOptions(options);

    }
    private void GetChildrenRecursive(Transform parent, List<Transform> piezas)
    {
        try
        {
            Transform child = parent.GetChild(0);
            piezas.Add(child);
            GetChildrenRecursive(child, piezas);
        }catch{}
    }
    public void GenerateTextInputsLimits()
    {
        GameObject brazo_root = GameObject.FindGameObjectWithTag("brazo_base");
        List<Transform> ejes = new();
        GetChildrenRecursive(brazo_root.transform, ejes);
        
        for (int i = 0; i < ejes.Count; i++)
        {
            GameObject minimo = Instantiate(input_prefab, new Vector3(250, -100 - (i * 42), 0), Quaternion.identity);
            minimo.transform.SetParent(config_canvas.transform, false);

            
            GameObject maximo = Instantiate(input_prefab, new Vector3(400, -100 - (i * 42), 0), Quaternion.identity);
            maximo.transform.SetParent(config_canvas.transform, false);
            GameObject text_prefab = Instantiate(axis_text_prefab, new Vector3(150, -115 - (i * 42), 0), Quaternion.identity);
            text_prefab.transform.SetParent(config_canvas.transform, false);
            text_prefab.GetComponent<TextMeshProUGUI>().text = "LIMITE Eje " + (i + 1).ToString() + ":";
        }

    }

}
