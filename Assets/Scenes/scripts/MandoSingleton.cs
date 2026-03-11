using UnityEngine;

public class MandoSingleton : MonoBehaviour
{
    public static MandoSingleton Instance { get; private set; }
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public MandoController mandoController;
}
