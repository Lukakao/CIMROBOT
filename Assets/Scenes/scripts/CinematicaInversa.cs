using UnityEngine;
using UnityEditor;

public class CinematicaInversa : MonoBehaviour
{
    [SerializeField] PartController _base,_lower,_upper,_wrist;
    [SerializeField] Transform target,lowerJoint,upperJoint,baseJoint;
    float _x,_y,_z;
    float _a,_b,_c;
    public float moveSpeed = 10f;
    float prevUpperAng;
    bool inverseKinematic = false;

    Rigidbody targetrb;
    void Awake()
    {
        _a = (_wrist.transform.position - upperJoint.position).magnitude;
        _c = (upperJoint.position - lowerJoint.position).magnitude;
        targetrb = target.GetComponent<Rigidbody>();
    }

    public void MoveX(int direction)
    {
        _x = direction;
    }
    public void MoveY(int direction)
    {
        _y = direction;
    }
    public void MoveZ(int direction)
    {
        _z = direction;
    }
    public void StopMoving()
    {
        _x= 0;_y=0;_z=0;
    }
    public void ResetTarget()
    {
        target.position = _wrist.transform.position;
    }
    public void ToggleKinematics()
    {
        if(MandoSingleton.Instance.mandoController.takingInput) return;
        inverseKinematic = !inverseKinematic;
        target.position = _wrist.transform.position;
        MandoSingleton.Instance.mandoController.SetKinematic(inverseKinematic);
        string text = inverseKinematic ? "XYZ" : "JOINTS";
        MandoSingleton.Instance.mandoController.ShowTextInfo($"Modo: {text}",1.5f);
    }
    void FixedUpdate()
    {
        if(!MandoSingleton.Instance.mandoController.robotCanMove) return;
        if(!MandoSingleton.Instance.mandoController.inverseKinematic) return;
        targetrb.linearVelocity = new Vector3(_x,_y,_z) * Time.deltaTime * moveSpeed;
        
        Vector3 dir = target.position - baseJoint.position;
        float rawAngle = Mathf.Atan2(-dir.z, dir.x) * Mathf.Rad2Deg;
        _base.SetAngle(rawAngle);

        _b = (target.position - lowerJoint.position ).magnitude;
        _b = Mathf.Min(_b, _a + _c - 0.0001f); 

        float cosA = (_b*_b+_c*_c-_a*_a)/(2*_b*_c);
        cosA = Mathf.Clamp(cosA, -1f, 1f);
        float lowerInternal = Mathf.Acos(cosA) * Mathf.Rad2Deg;

        dir = target.position - lowerJoint.position;
        float horizontal = new Vector2(dir.x, dir.z).magnitude;
        float vertical = dir.y;
        float targetAngle = Mathf.Atan2(vertical, horizontal) * Mathf.Rad2Deg;
        
        float lowerAngle = lowerInternal + targetAngle;
        _lower.SetAngle(lowerAngle);
        

        float cosB = (_a*_a + _c*_c - _b*_b) / (2f*_a*_c);
        cosB = Mathf.Clamp(cosB, -1f, 1f);
        float upperAngle = Mathf.Acos(cosB) * Mathf.Rad2Deg;
        float height = (upperJoint.position-lowerJoint.position).y;
        float cosP = Mathf.Clamp(height / _c, -1f, 1f);
        float pAngle = Mathf.Acos(cosP) * Mathf.Rad2Deg;
        Vector3 mento = upperJoint.position-lowerJoint.position;
        mento.y = 0;
        float dot = Vector3.Dot(Quaternion.AngleAxis(_base.currentAngle,_base.ejeRotacion)*_base.baseDir, mento);
        float finalUpperAngle;
        if (dot > 0) finalUpperAngle = upperAngle - pAngle;
        else finalUpperAngle = upperAngle + pAngle;
        _upper.SetAngle(finalUpperAngle);

        //mantener muñeca en su inclinacion
        if(prevUpperAng == default)
        {
            prevUpperAng = finalUpperAngle;
            return;
        }
        float deltaUpper = finalUpperAngle - prevUpperAng;
        float finalMunecaAngle;
        float dotMuneca = Vector3.Dot(_wrist.transform.forward,Vector3.up);

        if(dotMuneca>0) finalMunecaAngle = _wrist.currentAngle + deltaUpper;
        else finalMunecaAngle = _wrist.currentAngle - deltaUpper;

        _wrist.SetAngle(finalMunecaAngle);
        prevUpperAng = finalUpperAngle;
    }
    #if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Vector3 center = _wrist.transform.position;

        Handles.color = Color.red;
        Handles.DrawLine(center, center + 3*_wrist.transform.forward, 5f);

        
        Handles.color = new Color(0.169f, 0.169f, 0.169f, 1.000f);
        Handles.DrawLine(baseJoint.position, lowerJoint.position, 10f);
        Handles.DrawLine(lowerJoint.position, upperJoint.position, 10f);
        Handles.DrawLine(upperJoint.position, _wrist.transform.position, 10f);

        Handles.color = new Color(0.906f, 0.980f, 0.227f, 1.000f);
        Handles.DrawLine(baseJoint.position, baseJoint.position + _base.baseDir * 2f, 10f);
    }   
    #endif
}


