using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundManager : MonoBehaviour
{
    private Transform camPos;
    public List<Transform> bgList;
    public Transform alwaysMoving;

    public float parallaxMin;
    public float parallaxMax;
    public Vector2 moverSpd;

    private void Awake()
    {
        camPos = Camera.main.transform;
    }

    private void FixedUpdate()
    {
        UpdateParallax();
        //UpdateMover();
        ParallaxLoop();
    }

    private void ParallaxLoop()
    {
        for (int i = 0; i < bgList.Count; i++)
        {
            Transform bg = bgList[i];
            Sprite sprite = bg.GetComponent<SpriteRenderer>().sprite;
            Texture2D texture = sprite.texture;
            float textureSizeX = texture.width / sprite.pixelsPerUnit;
            float textureSizeY = texture.height / sprite.pixelsPerUnit;
            float offsetX = (camPos.position.x - bg.transform.position.x) % textureSizeX;
            float offsetY = (camPos.position.y - bg.transform.position.y) % textureSizeY;

            if (Mathf.Abs(camPos.position.x - bg.transform.position.x) >= textureSizeX)
                bg.position = new Vector3(camPos.position.x + offsetX, bg.position.y, bg.position.z);

            if (Mathf.Abs(camPos.position.y - bg.transform.position.y) >= textureSizeY)
                bg.position = new Vector3(bg.position.x, camPos.position.y + offsetY, bg.position.z);
            
        }

    }

    private void UpdateParallax()
    {
        float speed = parallaxMin;
        for (int i = 0; i < bgList.Count; i++)
        {
            bgList[i].transform.position = camPos.transform.position * speed;
            speed -= ((parallaxMin - parallaxMax) / bgList.Count);
        }
    }

    private void UpdateMover()
    {
        alwaysMoving.transform.position += new Vector3(moverSpd.x,moverSpd.y,0);
    }

}
