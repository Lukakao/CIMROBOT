using UnityEngine;

public class FaceCamera : MonoBehaviour
{
    public Camera camerac;
    void LateUpdate()
    {
        transform.LookAt(transform.position + camerac.transform.forward);
    }
}
