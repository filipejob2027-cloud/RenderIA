using UnityEngine;

public class RotarObjeto : MonoBehaviour
{
    public float velocidade = 20f;

    void Update()
    {
        transform.Rotate(0, velocidade * Time.deltaTime, 0);
    }
}
