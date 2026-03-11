using UnityEngine;

public class MirrorRotation : MonoBehaviour
{
    [SerializeField] Transform other;
    void LateUpdate()
    {
        transform.rotation = other.rotation;
    }
}
