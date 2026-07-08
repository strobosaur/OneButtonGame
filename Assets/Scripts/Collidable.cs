using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[RequireComponent(typeof(BoxCollider2D))]
public class Collidable : MonoBehaviour
{
    public ContactFilter2D filter;
    protected CircleCollider2D collider;
    private Collider2D[] hits = new Collider2D[10];

    protected virtual void Start()
    {
        collider = GetComponent<CircleCollider2D>();
    }

    protected virtual void FixedUpdate()
    {
        collider.Overlap(filter, hits);
        for(int i = 0; i < hits.Length; i++)
        {
            if(hits[i] == null)
                continue;
            else
                OnCollide(hits[i]);
            hits[i] = null;
        }
    }

    protected virtual void OnCollide(Collider2D collided)
    {

    }
}