using UnityEngine;
using System.Collections;

public class ImageFeedback3D : InseilFeedback {

    public Vector3 position;
    public Vector3 orientation;
    public Vector2 scale;
    public Sprite[] sprites;
    private SpriteRenderer rend;
    private int index = 0;

    //Debug
    public float changeCD;
    private float timeOfLastChange;

	// Use this for initialization
	void Start () {

        transform.position = position;
        transform.rotation = Quaternion.Euler(orientation);
        transform.localScale = new Vector3(scale.x, scale.y, transform.localScale.z);
        rend = GetComponent<SpriteRenderer>();
        rend.sprite = sprites[0];	
	}
	
	// Update is called once per frame
	void Update () {

        if (Time.time - timeOfLastChange >= changeCD)
        {
            
            timeOfLastChange = Time.time;
            NextSprite();
        }
	
	}

    public void NextSprite()
    {
        index = (index + 1) % sprites.Length;
        rend.sprite = sprites[index];
    }
}
