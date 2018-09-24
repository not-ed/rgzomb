using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour {

    public enum COLOR {
        RED,GREEN,BLUE
    }

    public COLOR TYPE;

    [SerializeField]
    private int TRAVEL_SPEED; //How fast the bullet will move.
    private Rigidbody2D C_RIGID_BODY_2D; //The Rigidbody2D component attatched to the game object.

    private SpriteRenderer C_SPRITE_RENDERER; //The Sprite Renderer component attatched to the game object.


    void InitializeComponents() //Establishes the values for component-based constants.
    {
        C_RIGID_BODY_2D = gameObject.GetComponent<Rigidbody2D>();
        C_SPRITE_RENDERER = gameObject.GetComponent<SpriteRenderer>();
    }

    // Use this for initialization
    void Start () {
        InitializeComponents();
        C_RIGID_BODY_2D.velocity = transform.right * TRAVEL_SPEED;
        switch (TYPE)
        {
            case COLOR.RED:
                C_SPRITE_RENDERER.color = Color.red;
                break;
            case COLOR.GREEN:
                C_SPRITE_RENDERER.color = Color.green;
                break;
            case COLOR.BLUE:
                C_SPRITE_RENDERER.color = Color.blue;
                break;
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Wall"))
        {
            Destroy(gameObject);
        }

        
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.GetComponent<Zombie>())
        {
            if (TYPE == collision.gameObject.GetComponent<Zombie>().COLOR)
            {
                Destroy(collision.gameObject);
                collision.GetComponent<Zombie>().SpawnDeathObject();
            }
            Destroy(gameObject);
        }
    }

    // Update is called once per frame
    void Update () {
		
	}
}
