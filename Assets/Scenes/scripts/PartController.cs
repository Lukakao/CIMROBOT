#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

public class PartController : MonoBehaviour
{
    public Vector3 ejeRotacion;
    public float minAng = -45f;
    public float maxAng = 45f;
    public float minAngOut = 0f;
    public float maxAngOut = 180f;
    public float minValue = 0f;
    public float maxValue = 0f;
    public float radius = 2f;
    public float thickness = 3f;
    public float startingAngle;

    Quaternion initialRotation;
    public float currentAngle;
    public float range => maxAng - minAng;
    int direction;
    public int servoNum;

    public float GetCurrentAngle()
    {
        return currentAngle;
    }

    public void SetAngle(float angle)
    {
        currentAngle = Mathf.Clamp(angle, 0f, range);
        ApplyRotation();
    }

    void Awake()
    {
        initialRotation = transform.localRotation;
        currentAngle = startingAngle;
    }

    public void ChangeDir(int dir)
    {
        direction = dir;
    }

    void FixedUpdate()
    {
        if(!MandoSingleton.Instance.mandoController.robotCanMove) return;
        if (direction == 0) return;
        if(MandoSingleton.Instance.mandoController.inverseKinematic) return;
        currentAngle += MandoSingleton.Instance.mandoController.speed * Time.fixedDeltaTime * direction;
        currentAngle = Mathf.Clamp(currentAngle, 0f, range);

        ApplyRotation();
    }

    void ApplyRotation()
    {
        float finalAngle = currentAngle - startingAngle;
        transform.localRotation = initialRotation * Quaternion.AngleAxis(finalAngle, ejeRotacion.normalized);
        if(MandoSingleton.Instance.mandoController.inverseKinematic) return;
        MandoSingleton.Instance.mandoController.ShowTextContext($"SERVO [{servoNum}]:      {currentAngle.ToString("F0")}°", 1f);
    }
public Vector3 baseDir;
#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Handles.zTest = UnityEngine.Rendering.CompareFunction.Always;

        Vector3 center = transform.position;
        Vector3 axis = transform.TransformDirection(ejeRotacion.normalized);
        baseDir = Vector3.Cross(axis, Vector3.up);
        if (baseDir.sqrMagnitude < 0.0001f) 
        baseDir = Vector3.Cross(axis, Vector3.forward);
        baseDir.Normalize();

        Handles.color = Color.red;
        Handles.DrawWireDisc(center, axis, radius, thickness);

        Handles.color = Color.blue;
        Handles.DrawLine(center,center + baseDir*radius);

        Handles.color = Color.magenta;
        Vector3 minDir = Quaternion.AngleAxis(minAng, axis) * baseDir;
        Vector3 maxDir = Quaternion.AngleAxis(maxAng, axis) * baseDir;
        Handles.DrawLine(center, center + minDir * radius, thickness);
        Handles.DrawLine(center, center + maxDir * radius, thickness);

        Handles.color = Color.cyan;
        Vector3 currentDir = Quaternion.AngleAxis(minAng + currentAngle, axis) * baseDir;
        Handles.DrawLine(center, center + currentDir * radius, thickness + 2f);
        
        Handles.color = new Color(0.439f, 0.980f, 0.000f, 1.000f);

        Vector3 startingDir = Quaternion.AngleAxis(startingAngle+minAng, axis) * baseDir;
        Handles.DrawLine(center + startingDir * 0.15f, center + startingDir * (radius + 0.7f), thickness);

        Handles.color = Color.white;
        Handles.Label(center + currentDir * (radius + 0.5f),currentAngle.ToString("F1") + "°"
        );
    }
#endif
}