using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BloodPS : MonoBehaviour
{
    public ParticleSystem ps;
    public float angle;
    public ParticleSystem.ShapeModule shapePS;
    // Start is called before the first frame update
    void Start()
    {
        shapePS = ps.shape;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetAngle(Vector2 dir)
    {
        int xdir = Mathf.RoundToInt(dir.x);
        angle = Vector2.Angle(Vector2.up, dir.normalized);
        shapePS.rotation = new Vector3(0f,(angle/3)*xdir,0f);
    }
}
