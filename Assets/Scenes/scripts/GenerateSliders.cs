using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.IO;
using System.Globalization;
using UnityEngine.EventSystems;
using UnityEngine.PlayerLoop;

public class GenerateSliders : MonoBehaviour
{
    [SerializeField] List<PartController> parts;

    [SerializeField] GameObject prefabslider, resetButtton;

    [Header("Layout Settings")]
    [SerializeField] float leftColumnX = 65f;
    [SerializeField] float rightColumnX = 215f;
    [SerializeField] float startY = 300f;
    [SerializeField] float rowSpacing = 45f;

    void Awake()
    {
        ReadValues();
    }

    void Start()
    {
        Generate();
        UpdateVisibility();
    }

    void Generate()
    {
        foreach (Transform go in transform)
        {
            Destroy(go.gameObject);
        }
        for (int i = 0; i < parts.Count; i++)
        {
            CreateSlider(i, parts[i]);
        }
    }
    bool  viz = false;
    void ToggleViz()
    {
        viz = !viz;
        UpdateVisibility();
    }

    void UpdateVisibility()
    {
        foreach(Transform t in transform)
        {
            t.gameObject.SetActive(viz);
        }
        GetComponent<Image>().enabled = viz;
        resetButtton.SetActive(viz);
    }
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Tab))ToggleViz();
    }

    void CreateSlider(int index, PartController part)
    {
        // 1. Min Pulse Value Slider
        GameObject samin = Instantiate(prefabslider, transform);
        RectTransform rect = samin.GetComponent<RectTransform>();
        rect.position = new Vector2(leftColumnX, -rowSpacing * index + startY);

        Slider slider = samin.GetComponent<Slider>();
        slider.minValue = 0;
        slider.maxValue = Mathf.Max(800f, part.minValue, part.maxValue);
        slider.value = part.minValue;
        TextMeshProUGUI tmp = slider.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        tmp.text = part.minValue.ToString(CultureInfo.InvariantCulture);
        slider.onValueChanged.AddListener(value =>
        {
            parts[index].minValue = value;
            tmp.text = value.ToString(CultureInfo.InvariantCulture);
        });
        AddEndChangeListener(samin, SaveValues);

        // 2. Max Pulse Value Slider
        GameObject samax = Instantiate(prefabslider, transform);
        RectTransform rect2 = samax.GetComponent<RectTransform>();
        Slider slider2 = samax.GetComponent<Slider>();
        rect2.position = new Vector2(leftColumnX, -rowSpacing * index - 15 + startY);
        TextMeshProUGUI tmp2 = slider2.transform.GetChild(0).GetComponent<TextMeshProUGUI>();

        slider2.minValue = 0;
        slider2.maxValue = Mathf.Max(800f, part.minValue, part.maxValue);
        slider2.value = part.maxValue;
        tmp2.text = part.maxValue.ToString(CultureInfo.InvariantCulture);

        slider2.onValueChanged.AddListener(value =>
        {
            parts[index].maxValue = value;
            tmp2.text = value.ToString(CultureInfo.InvariantCulture);
        });
        AddEndChangeListener(samax, SaveValues);
        
        slider.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = "s" + index + " min:";
        slider2.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = "s" + index + " max:";

        // 3. Min Angle Out Slider (0 to 180, placed to the right of min pulse)
        GameObject saminAng = Instantiate(prefabslider, transform);
        RectTransform rect3 = saminAng.GetComponent<RectTransform>();
        rect3.position = new Vector2(rightColumnX, -rowSpacing * index + startY);

        Slider slider3 = saminAng.GetComponent<Slider>();
        slider3.minValue = 0f;
        slider3.maxValue = 180f;
        slider3.value = part.minAngOut;
        TextMeshProUGUI tmp3 = slider3.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        tmp3.text = part.minAngOut.ToString(CultureInfo.InvariantCulture);

        slider3.onValueChanged.AddListener(value =>
        {
            parts[index].minAngOut = value;
            tmp3.text = value.ToString(CultureInfo.InvariantCulture);
        });
        AddEndChangeListener(saminAng, SaveValues);

        // 4. Max Angle Out Slider (0 to 180, placed to the right of max pulse)
        GameObject samaxAng = Instantiate(prefabslider, transform);
        RectTransform rect4 = samaxAng.GetComponent<RectTransform>();
        Slider slider4 = samaxAng.GetComponent<Slider>();
        rect4.position = new Vector2(rightColumnX, -rowSpacing * index - 15 + startY);
        TextMeshProUGUI tmp4 = slider4.transform.GetChild(0).GetComponent<TextMeshProUGUI>();

        slider4.minValue = 0f;
        slider4.maxValue = 180f;
        slider4.value = part.maxAngOut;
        tmp4.text = part.maxAngOut.ToString(CultureInfo.InvariantCulture);

        slider4.onValueChanged.AddListener(value =>
        {
            parts[index].maxAngOut = value;
            tmp4.text = value.ToString(CultureInfo.InvariantCulture);
        });
        AddEndChangeListener(samaxAng, SaveValues);

        slider3.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = "s" + index + " minOut:";
        slider4.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = "s" + index + " maxOut:";
    }

    void AddEndChangeListener(GameObject go, Action action)
    {
        EventTrigger trigger = go.GetComponent<EventTrigger>();
        if (trigger == null) trigger = go.AddComponent<EventTrigger>();

        EventTrigger.Entry pointerUp = new EventTrigger.Entry();
        pointerUp.eventID = EventTriggerType.PointerUp;
        pointerUp.callback.AddListener((data) => { action(); });
        trigger.triggers.Add(pointerUp);

        EventTrigger.Entry endDrag = new EventTrigger.Entry();
        endDrag.eventID = EventTriggerType.EndDrag;
        endDrag.callback.AddListener((data) => { action(); });
        trigger.triggers.Add(endDrag);
    }

    string[] GetFilePaths()
    {
        return new string[]
        {
            Path.Combine(Application.dataPath, "valores.txt"),
            Path.Combine(Directory.GetParent(Application.dataPath).FullName, "valores.txt"),
            Path.Combine(Application.persistentDataPath, "valores.txt")
        };
    }

    void ReadValues()
{
    try
    {
        if (parts == null || parts.Count == 0)
        {
            Debug.LogWarning("[ReadValues] parts es null o esta vacio, no se puede cargar nada.");
            return;
        }

        string foundPath = null;
        string[] lines = null;

        foreach (string path in GetFilePaths())
        {
            Debug.Log($"[ReadValues] Probando ruta: {path} -> existe: {File.Exists(path)}");
            if (File.Exists(path))
            {
                string[] candidateLines = File.ReadAllLines(path);
                Debug.Log($"[ReadValues] Lineas en {path}: {candidateLines.Length}, primera linea: '{(candidateLines.Length > 0 ? candidateLines[0] : "")}'");
                if (candidateLines.Length > 0 && !string.IsNullOrWhiteSpace(candidateLines[0]))
                {
                    foundPath = path;
                    lines = candidateLines;
                    break;
                }
            }
        }

        if (lines != null && lines.Length > 0)
        {
            for (int i = 0; i < lines.Length && i < parts.Count; i++)
            {
                if (parts[i] == null)
                {
                    Debug.LogWarning($"[ReadValues] parts[{i}] es null, se salta.");
                    continue;
                }

                string[] values = lines[i].Split((char[])null, StringSplitOptions.RemoveEmptyEntries);
                Debug.Log($"[ReadValues] Linea {i}: '{lines[i]}' -> {values.Length} valores");

                if (values.Length >= 2 &&
                    float.TryParse(values[0], NumberStyles.Float, CultureInfo.InvariantCulture, out float min) &&
                    float.TryParse(values[1], NumberStyles.Float, CultureInfo.InvariantCulture, out float max))
                {
                    // Se asignan tal cual vienen del archivo, sin validar ni reordenar:
                    // min puede ser mayor que max (ej. servo montado invertido).
                    parts[i].minValue = min;
                    parts[i].maxValue = max;
                }
                else
                {
                    Debug.LogWarning($"[ReadValues] Linea {i}: no se pudo parsear min/max.");
                }

                if (values.Length >= 4 &&
                    float.TryParse(values[2], NumberStyles.Float, CultureInfo.InvariantCulture, out float minAngOut) &&
                    float.TryParse(values[3], NumberStyles.Float, CultureInfo.InvariantCulture, out float maxAngOut))
                {
                    parts[i].minAngOut = Mathf.Clamp(minAngOut, 0f, 240f);
                    parts[i].maxAngOut = Mathf.Clamp(maxAngOut, 0f, 240f);
                }
            }
            Debug.Log($"[ReadValues] Valores cargados desde: {foundPath}");
        }
        else
        {
            Debug.LogWarning("[ReadValues] No se encontro ningun archivo valido en ninguna de las 3 rutas.");
        }
    }
    catch (Exception ex)
    {
        Debug.LogError($"Error al leer valores.txt: {ex.Message}");
    }
}

    void SaveValues()
    {
        try
        {
            if (parts == null || parts.Count == 0) return;

            string[] lines = new string[parts.Count];
            for (int i = 0; i < parts.Count; i++)
            {
                if (parts[i] == null) continue;
                lines[i] = $"{parts[i].minValue.ToString(CultureInfo.InvariantCulture)} {parts[i].maxValue.ToString(CultureInfo.InvariantCulture)} {parts[i].minAngOut.ToString(CultureInfo.InvariantCulture)} {parts[i].maxAngOut.ToString(CultureInfo.InvariantCulture)}";
            }

            foreach (string path in GetFilePaths())
            {
                try
                {
                    string dir = Path.GetDirectoryName(path);
                    if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                    {
                        Directory.CreateDirectory(dir);
                    }
                    File.WriteAllLines(path, lines);
                    Debug.Log($"Valores guardados en: {path}");
                }
                catch (Exception writeEx)
                {
                    Debug.LogWarning($"No se pudo guardar en {path}: {writeEx.Message}");
                }
            }

#if UNITY_EDITOR
            UnityEditor.AssetDatabase.Refresh();
#endif
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error al guardar valores.txt: {ex.Message}");
        }
    }

    public void SetDefault()
    {
        float[] defaultMinValues = { 594.9908f, 692.4776f, 187.8139f, 495.1794f, 26.80788f, 505.8763f };
        float[] defaultMaxValues = { 66.93839f, 12.56706f, 530.6007f, 85.68136f, 414.6981f, 117.5925f };
        float[] defaultMinAngOut = { 0f, 0f, 0f, 0f, 27.41813f, 0f };
        float[] defaultMaxAngOut = { 180f, 166.2909f, 180f, 180f, 180f, 180f };

        int count = Mathf.Min(parts.Count, defaultMinValues.Length);
        for (int i = 0; i < count; i++)
        {
            if (parts[i] == null) continue;
            parts[i].minValue = defaultMinValues[i];
            parts[i].maxValue = defaultMaxValues[i];
            parts[i].minAngOut = defaultMinAngOut[i];
            parts[i].maxAngOut = defaultMaxAngOut[i];
        }

        SaveValues();
        Generate();
    }
}
