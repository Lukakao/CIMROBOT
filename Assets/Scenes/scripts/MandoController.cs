using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection.Emit;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum Modo
{
    NORMAL,
    RECORD,
    LOADINGMOVE,
    EXECUTING,
    CHANGINGSPEED,
    MAKINGSEQUENCE
}

public class MandoController : MonoBehaviour
{    
    public bool inverseKinematic = false;
    public float speed = 100f;
    public bool robotCanMove = true;
    Dictionary<string,ServoData> positions = new Dictionary<string, ServoData>();
    [SerializeField] List<PartController> servos;
    [SerializeField] TextMeshProUGUI tmpInput, tmpContext, tmpInfo;
    Coroutine c_context,c_info,c_input, executionCoroutine;
    public bool takingInput = false;
    Modo modo = Modo.NORMAL;
    [SerializeField] Transform wristJoint;
    [SerializeField] CinematicaInversa cinematicaInversa;
    List<GameObject> labelspool = new();
    List<GameObject> spheresPool = new();
    int initialLabelsCount = 5;
    [SerializeField] GameObject labelPrefab,spherePrefab;

    List<string> seq = new();
    List<string> tempseq = new();

    public void SetKinematic(bool state)
    {
        inverseKinematic = state;
    }
    void Awake()
    {
        for (int i = 0; i < initialLabelsCount; i++)
        {
            GameObject n = Instantiate(labelPrefab, new Vector3(0,-1000,0) ,Quaternion.identity);
            n.SetActive(false);
            labelspool.Add(n);

            GameObject b = Instantiate(spherePrefab, new Vector3(0,-1000,0) ,Quaternion.identity);
            b.SetActive(false);
            spheresPool.Add(b);
        }
        ResetToNormal();
        positions = new();
    }

    public void ShowTextContext(string mess, float duration)
    {
        if(c_context!=null) StopCoroutine(c_context); 
        c_context = StartCoroutine(ShowTextCoroutine(tmpContext,mess,duration));
    }
    public void ShowTextInfo(string mess, float duration)
    {
        if(c_info!=null) StopCoroutine(c_info); 
        c_info = StartCoroutine(ShowTextCoroutine(tmpInfo,mess,duration));
    }
    IEnumerator ShowTextCoroutine(TextMeshProUGUI tmp, string mess, float duration)
    {
        string original = "";
        tmp.text = mess;
        yield return new WaitForSeconds(duration);
        tmp.text = original;
    }

    void SavePosition()
    {
        ServoData data = new(wristJoint.position, servos[0].currentAngle,servos[1].currentAngle,servos[2].currentAngle,
        servos[3].currentAngle,servos[4].currentAngle,servos[5].currentAngle);
        positions[GetInput()] = data;
        ShowTextInfo("Grabado exitoso",1.5f);
        UpdateGizmos();
    }

    public void SpeedUp()
    {
        if(modo != Modo.CHANGINGSPEED) return;
        speed = Mathf.Clamp(speed+1,1,100);
        ChangeInfo("Vel. actual: " + speed);
    }
    public void SpeedDown()
    {
        if(modo != Modo.CHANGINGSPEED) return;
        speed = Mathf.Clamp(speed-1,1,100);
        ChangeInfo("Vel. actual: " + speed);
    }

    public void ExecuteSequence()
    {
        executionCoroutine = StartCoroutine(ExecuteSequenceRoutine());
    }

    IEnumerator ExecuteSequenceRoutine()
    {
        modo = Modo.EXECUTING;
        ChangeContext("Ejecutando...");
        foreach (var s in seq)
        {
            if (!positions.TryGetValue(s, out ServoData target))
                continue;

            float duration = this.duration; 

            bool finished = false;
            LeanTween.value(gameObject, servos[0].GetCurrentAngle(), target.s1, duration)
                .setOnUpdate(val => servos[0].SetAngle(val));

            LeanTween.value(gameObject, servos[1].GetCurrentAngle(), target.s2, duration)
                .setOnUpdate(val => servos[1].SetAngle(val));

            LeanTween.value(gameObject, servos[2].GetCurrentAngle(), target.s3, duration)
                .setOnUpdate(val => servos[2].SetAngle(val));

            LeanTween.value(gameObject, servos[3].GetCurrentAngle(), target.s4, duration)
                .setOnUpdate(val => servos[3].SetAngle(val));

            LeanTween.value(gameObject, servos[4].GetCurrentAngle(), target.s5, duration)
                .setOnUpdate(val => servos[4].SetAngle(val));

            LeanTween.value(gameObject, servos[5].GetCurrentAngle(), target.s6, duration)
                .setOnUpdate(val => servos[5].SetAngle(val))
                .setOnComplete(() => finished = true);

            yield return new WaitUntil(() => finished);
            yield return new WaitForSeconds(0.5f);
        }
        cinematicaInversa.ResetTarget();
        ShowTextInfo("Terminado", 1.5f);
        ResetToNormal();
    }


    float duration = 1.5f;

    void ChangeSpeed()
    {
        if (int.TryParse(tmpInput.text, out int result))
        {
            if(result < 1 && result > 100){
                ShowTextInfo("vel. no valida",1.5f);
                return;
            }
            speed = result;
            ShowTextInfo("ok",1f);
            return;
        }
        ShowTextInfo("vel. no valida",1.5f);
    }

    public void GoPosition()
    {
        //validar que exista en dicitonario
        string index = tmpInput.text;
        ServoData target;
        try
        {
            target = positions[index];
        }
        catch
        {
            ShowTextInfo("POSICION INVALIDA",1.5f);
            return;
        }
        modo = Modo.EXECUTING;
        takingInput = false;
        robotCanMove = false;
        ChangeContext("ejecutando...");
        LeanTween.value(gameObject, servos[0].GetCurrentAngle(), target.s1, duration)
        .setOnUpdate((float val) => servos[0].SetAngle(val));

        LeanTween.value(gameObject, servos[1].GetCurrentAngle(), target.s2, duration)
            .setOnUpdate((float val) => servos[1].SetAngle(val));

        LeanTween.value(gameObject, servos[2].GetCurrentAngle(), target.s3, duration)
            .setOnUpdate((float val) => servos[2].SetAngle(val));

        LeanTween.value(gameObject, servos[3].GetCurrentAngle(), target.s4, duration)
            .setOnUpdate((float val) => servos[3].SetAngle(val));

        LeanTween.value(gameObject, servos[4].GetCurrentAngle(), target.s5, duration)
            .setOnUpdate((float val) => servos[4].SetAngle(val));

        LeanTween.value(gameObject, servos[5].GetCurrentAngle(), target.s6, duration)
            .setOnUpdate((float val) => servos[5].SetAngle(val)).setOnComplete(()=>ResetToNormal());
    }
    string GetInput()
    {
        return tmpInput.text;
    }

    public void Enter()
    {
        //do action
        switch (modo)
        {
            case Modo.RECORD: SavePosition(); break;
            case Modo.LOADINGMOVE: GoPosition(); break;
            case Modo.CHANGINGSPEED: ChangeSpeed(); break;
            case Modo.MAKINGSEQUENCE: AddToSequence(); return;
        }
        ResetToNormal();
    }

    public void Abort()
    {
        StopCoroutine(executionCoroutine);
        ResetToNormal();
        ChangeInfo("");
        ShowTextInfo("Abortado",1.5f);
        
    }

    void AddToSequence()
    {   
        if(tmpInput.text.Length == 0)
        {
            //secuencia creada
            if(tempseq.Count == 0) return;
            seq = tempseq;
            ResetToNormal();
            ShowTextInfo("Creado",1.5f);
            return;
        }
        if (positions.ContainsKey(tmpInput.text))
        {
            tempseq.Add(tmpInput.text);
            ShowTextInfo("Agregado",0.5f);
            tmpInput.text = "";
        }
        else
        {
            tmpInput.text = "";
            ShowTextInfo("Indice no valido",1.5f);
        }
    }



    void ResetToNormal()
    {
        modo = Modo.NORMAL;
        robotCanMove = true;
        tmpInput.text = "";
        takingInput = false;
        ChangeContext("");
        Debug.Log("reset");
    }
    public void EnterSavePositionLoop()
    {
        robotCanMove = false;
        takingInput = true;
        modo = Modo.RECORD;
        tmpInput.text = "";
        ChangeContext("GRABAR:");
    }
    public void EnterMovePositionLoop()
    {
        robotCanMove = false;
        takingInput = true;
        modo = Modo.LOADINGMOVE;
        tmpInput.text = "";
        ChangeContext("MOVER:");
    }

    public void EnterCreateSeqPositionLoop()
    {
        tempseq = new();
        robotCanMove = false;
        takingInput = true;
        modo = Modo.MAKINGSEQUENCE;
        tmpInput.text = "";
        ChangeContext("AGREGAR:");
    }

    public void EnterVelPositionLoop()
    {
        robotCanMove = false;
        takingInput = true;
        modo = Modo.CHANGINGSPEED;
        tmpInput.text = "";
        ChangeContext("VELOCIDAD:");
        ChangeInfo("Vel. actual: " + speed);
    }


    public void ChangeContext(string mess)
    {
        if(c_context!=null) StopCoroutine(c_context); 
        tmpContext.text = mess;
    }

    public void ChangeInfo(string mess)
    {
        if(c_info!=null) StopCoroutine(c_info); 
        tmpInfo.text = mess;
    }

    public void ReceiveInput(string str)
    {
        if(!takingInput)return;
        if(tmpInput.text.Length <3) tmpInput.text += str;
    }

    //display positions
    void UpdateGizmos()
    {
        int index = 0;
        foreach (var item in labelspool)
        {
            item.SetActive(false);
        }
        foreach (var item in spheresPool)
        {
            item.SetActive(false);
        }
        foreach (var pair in positions)
        {
            if (index == labelspool.Count-1)
            {
                GameObject n = Instantiate(labelPrefab, new Vector3(0,-1000,0) ,Quaternion.identity);
                n.SetActive(false);
                labelspool.Add(n);

                GameObject b = Instantiate(spherePrefab, new Vector3(0,-1000,0) ,Quaternion.identity);
                b.SetActive(false);
                spheresPool.Add(b);
            }

            spheresPool[index].transform.position = pair.Value.targetPos;
            spheresPool[index].SetActive(true);

            labelspool[index].transform.position = pair.Value.targetPos;
            labelspool[index].GetComponent<TextMeshPro>().text = pair.Key;
            labelspool[index].SetActive(true);
            index++;
        }
    }


}


public struct ServoData
{
    public Vector3 targetPos;
    public float s1;
    public float s2;
    public float s3;
    public float s4;
    public float s5;
    public float s6;

    public ServoData(Vector3 targetPos,float s1, float s2, float s3, float s4, float s5, float s6)
    {
        this.targetPos = targetPos;
        this.s1 = s1;
        this.s2 = s2;
        this.s3 = s3;
        this.s4 = s4;
        this.s5 = s5;
        this.s6 = s6;
    }
}

//todo: f i know