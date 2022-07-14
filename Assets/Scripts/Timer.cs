using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Timer : MonoBehaviour
{
    public float lastActivated;
    public float counter;
    public float duration;
    public float fraction;
    public bool timerEnd;

    // Start is called before the first frame update
    void Start()
    {
        lastActivated = Time.deltaTime;
        timerEnd = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (!timerEnd) {
            fraction = 1 - ((Time.time - lastActivated) / duration);
            if (Time.time - lastActivated > duration){
                timerEnd = true;
                fraction = 0f;
            }
        }
    }

    public void SetNewTime(float dur)
    {
        duration = dur;
        lastActivated = Time.time;
        timerEnd = false;
    }
}
