using UnityEngine;

public class JumpFlash : MonoBehaviour
{

    private SpriteRenderer sr;
    public float alphaMod;
    private float rotationMod;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        alphaMod = 0.001f;
        rotationMod = 0f;
    }

    // Update is called once per frame
    void Update()
    {
        rotationMod += 1.5f;
        if (alphaMod < 1f)
        {
            alphaMod *= 1.5f;
        }
        else
        {
            alphaMod = 0.001f;
        }

        transform.rotation = Quaternion.Euler(0f, 0f, rotationMod);
    }
}
