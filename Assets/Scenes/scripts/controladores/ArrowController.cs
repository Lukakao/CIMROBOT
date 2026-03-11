using UnityEngine;

public class ArrowController : MonoBehaviour
{
    Transform target;
    Transform mainCam;
    void Start()
    {
        mainCam = Camera.main.transform;
        target = GameObject.FindGameObjectWithTag("brazo_base").transform;
    }

    void LateUpdate()
    {
        transform.rotation = target.rotation;
    }
}
