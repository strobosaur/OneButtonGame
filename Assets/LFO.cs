using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LFO
{
    public float speed = 0.01f;
    public float value = 0f;
    private bool up = true;

    // Update is called once per frame
    public void UpdateLfo()
    {
        if (up)
            value += speed;
        else
            value -= speed;

        if (value >= 1f) {
            value = 1f;
            up = false;
        } else if (value <= 0f) {
            value = 0f;
            up = true;
        }
    }
}