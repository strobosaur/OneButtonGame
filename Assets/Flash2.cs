using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flash2 : MonoBehaviour
{
    private float startTime;
    private float duration;
    // Start is called before the first frame update
    void Start()
    {
        startTime = Time.time;
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.time - startTime > duration){
            Destroy(gameObject);
        }
    }
}
