using System.Collections.Generic;
using UnityEngine;

public class CollisionDetection : MonoBehaviour
{
    [SerializeField] List<PartController> servos;
    [SerializeField] Transform iktarget;
    [SerializeField] CinematicaInversa cinematicaInversa;
    float prevUpper,prevLower;
    Vector3 prevIKtarget;
    void Awake()
    {
        prevLower = servos[1].currentAngle;
        prevUpper = servos[2].currentAngle;
        prevIKtarget = iktarget.position;
    }
    void LateUpdate()
    {
        Vector3 currentDir = Quaternion.AngleAxis(servos[1].minAng + servos[1].currentAngle, transform.TransformDirection(servos[1].ejeRotacion.normalized)) * servos[1].baseDir;
        Vector3 currentDirupper = Quaternion.AngleAxis(servos[2].minAng + servos[2].currentAngle, transform.TransformDirection(servos[2].ejeRotacion.normalized)) * servos[2].baseDir;
        float angle = Vector3.Angle(currentDir, currentDirupper);
        // if(angle < 30 || angle > 130)
        // {
        //     MandoSingleton.Instance.mandoController.ShowTextContext("limite interno",1f); //pinch
        //     servos[1].currentAngle = prevLower;
        //     servos[2].currentAngle = prevUpper;
        //     cinematicaInversa.StopMoving();
        //     Vector3 dif = iktarget.position - prevIKtarget;
        //     iktarget.position = prevIKtarget - dif.normalized * 0.02f;
        // }
        // prevLower = servos[1].currentAngle;
        // prevUpper = servos[2].currentAngle;
        // prevIKtarget = iktarget.position;
    }
}
