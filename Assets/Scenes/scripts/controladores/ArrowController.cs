using UnityEngine;

public class ArrowController : MonoBehaviour
{
    Transform target;
    void Start()
    {
        target = GameObject.FindGameObjectWithTag("brazo_base").transform;
    }

    void LateUpdate()
    {
        transform.rotation = target.rotation * Quaternion.Euler(0, 0, 90);
    }
}
