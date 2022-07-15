using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flash : MonoBehaviour
{
    public SpriteRenderer sr;
    private float duration;
    private float factor;
    private float startTime;
    private float scaleModifier;
    private float scale;

    private void Start()
    {
        transform.parent = null;
        transform.Translate(Random.insideUnitCircle * 0.325f);
        //scaleModifier = Random.Range(0.5f,1.25f);
        //sr = GetComponent<SpriteRenderer>();
        startTime = Time.time;
        duration = Random.Range(1f,3f);
    }

    private void Update()
    {
        factor = Mathf.Max(0f, 1 - ((Time.time - startTime) / duration));
        sr.color = new Color(sr.color.r,sr.color.b,sr.color.g, sr.color.a * (factor * Random.Range(0.25f,0.75f)));
        //scale = Mathf.Lerp(factor,scaleModifier,0.5f);
        //Debug.Log("Scale: " + scale + "\nFactor: " + factor);
        //transform.localScale = new Vector3(scale, scale, 0f);

        if (Time.time - startTime > duration) {
            Destroy(gameObject);
        }
    }
}
