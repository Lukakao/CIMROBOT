using UnityEngine;

public class KeepParallel : MonoBehaviour
{
    /*                             robot pieza superior
    robot                        A                B                E
           A   B             |      ___________   O  ___/          __ 
    E|=====o___o/###|         |  O  |          \____/            /  O \   --> barra qe conecta servo inferior
           |   |               \__ /                            |      |        con piezo superior
           o___o                                                |      |
           C   D                                                |      |

    */
    [SerializeField] Transform other;
    void LateUpdate()
    {

        transform.rotation = other.rotation;
    }
}
