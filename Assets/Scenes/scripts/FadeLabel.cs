using TMPro;
using UnityEngine;

public class FadeLabel : MonoBehaviour
{
    [SerializeField] Transform center,cam;
    TextMeshPro text;
    Vector3 camdir;

    void Awake()
    {
        
        text = GetComponent<TextMeshPro>();
    }
    void Update()
    {
        Vector3 dir = transform.position - center.position;
        camdir = transform.position - cam.position;
        float value = Mathf.Abs(Vector3.Dot(dir.normalized,camdir.normalized));
        Color c = text.color;
        c.a = 1-value;
        text.color = c;
    }
}
