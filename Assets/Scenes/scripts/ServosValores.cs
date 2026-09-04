using TMPro;
using UnityEngine;

public class ServosValores : MonoBehaviour
{
 
    [SerializeField] PartController s1,s2,s3,s4,s5,s6;
    [SerializeField] TextMeshProUGUI tmp;


    void Update()
    {
        tmp.text = $"s1: {s1.currentAngle}\ns2: {s2.currentAngle}\ns3: {s3.currentAngle}\ns4: {s4.currentAngle}\ns5: {s5.currentAngle}\ns6: {s6.currentAngle}";
    }
}
