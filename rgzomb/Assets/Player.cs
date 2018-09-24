using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Ammo
{
    public int red;
    public int green;
    public int blue;

    public Ammo(int r, int g, int b)
    {
        red = r;
        green = g;
        blue = b;
    }

    public void UpdateCount(int red_amount, int green_amount, int blue_amount)
    {
        red += red_amount;
        red = Mathf.Clamp(red,0,99);
        green += green_amount;
        green = Mathf.Clamp(green, 0, 99);
        blue += blue_amount;
        blue = Mathf.Clamp(blue, 0, 99);
    }
}


public class Player : MonoBehaviour {

    private enum DIRECTION
    {
        LEFT, RIGHT, UP, DOWN
    }

    private DIRECTION currentDirection = DIRECTION.RIGHT; //The current direction in which the player is moving or facing.
    private Vector2 currentDesiredPosition; //The destination position as to where the player wishes to go.
    private bool isAlive = true;
    private bool isAbleToMove = true; //Whether or not the player is able to move.
    private bool isAbleToFire = true; //Whether or not the player is able to fire.
    private int remainingLives = 3; //How many lives the player has left.

    private Ammo currentAmmo = new Ammo(10, 10, 10);

    [SerializeField]
    private Text hudTextRedAmmo, hudTextGreenAmmo, hudTextBlueAmmo,hudTextMsgCentre;
    [SerializeField]
    private SpriteRenderer[] hudLifeIcons;

    //Constant Variables
    [SerializeField]
    private float MOVEMENT_SPEED; //The speed in which the player will move (pixels per second)
    private int GRID_SIZE_REFERENCE = 16; //The reference size of the grid in pixels which the player will be aligned to when moving.
    private Rigidbody2D C_RIGID_BODY_2D; //The Rigidbody2D component attatched to the game object.
    private Animator C_ANIMATOR; //The Animator component attatched to the game object.
    private int LAYERMASK_WALL; // The Layer Mask for the Layer named "Wall".
    [SerializeField]
    private GameObject BULLET;
    [SerializeField]
    private float FIRE_COOLDOWN_TIME; //How much time needs to pass until the player can fire again.
    [SerializeField]
    private int MAX_BULLET_COUNT; //How many bullets can be on screen at once.
    private Vector2 STARTING_POSITION; //The postion the player is in when starting, used to place them back when  respawning.
    private AudioSource AUDIO_MASTER; //The SoundManager in the game, only one audiosource should exist, which all objects will feed in to.
    private MainMenu MAIN_MENU; //The Main Menu.
    [SerializeField]
    private GameObject MENU_TRANSITION_COVER; //the object that blocks the screen upon loading back into the main menu on a game over.

    //sound effects
    [SerializeField]
    private AudioClip AC_WALK, AC_FIRE, AC_DEATH, AC_GAMESTART;

    private ZombieSpawner[] EXISTING_ZOMBIE_SPAWNERS;
    private AmmoSpawner[] EXISTING_AMMO_SPAWNERS;


    void InitializeComponents() //Establishes the values for component-based constants.
    {
        C_RIGID_BODY_2D = gameObject.GetComponent<Rigidbody2D>();
        C_ANIMATOR = gameObject.GetComponent<Animator>();
        EXISTING_ZOMBIE_SPAWNERS = FindObjectsOfType<ZombieSpawner>();
        EXISTING_AMMO_SPAWNERS = FindObjectsOfType<AmmoSpawner>();
        AUDIO_MASTER = FindObjectOfType<AudioSource>();
        MAIN_MENU = FindObjectOfType<MainMenu>();
    }

    void StartAllSpawners()
    {
        foreach (var item in EXISTING_ZOMBIE_SPAWNERS)
        {
            item.StartZombieSpawns();
        }

        foreach (var item in EXISTING_AMMO_SPAWNERS)
        {
            item.PrepareNextItem(false);
        }
    }

    void PauseAllSpawners()
    {
        foreach (var item in EXISTING_ZOMBIE_SPAWNERS)
        {
            item.PauseZombieSpawns();
        }

        foreach (var item in EXISTING_AMMO_SPAWNERS)
        {
            item.Stop();
        }

    }

    void Awake() //Called before the first frame.
    {
        InitializeComponents();
        UpdateAmmoCounterDisplays();
        STARTING_POSITION = transform.position; //Sets the player's grid position to their world position.
        ReturnToStart();
        ScoreCounter.currentScore = 0;
    }

    void ReturnToStart()
    {
        transform.position = STARTING_POSITION;
        currentDesiredPosition = STARTING_POSITION;
    }

    // Use this for initialization
    void Start () {
        //Grabs the layer mask for the "Wall" layer for use later on. Has to be done in start as the value cannot be correctly gathered using Awake.
        LAYERMASK_WALL = LayerMask.GetMask("Wall");

        StartCoroutine("StartGame");

        StartAllSpawners();
    }

    void StopAudio()
    {
        AUDIO_MASTER.gameObject.SetActive(false);
        AUDIO_MASTER.gameObject.SetActive(true);
    }


    // Sets the player's destination to move to based on what direction was chosen, and plays the appropriate walk animation.
    void Move(DIRECTION desired_direction)
    {
        switch (desired_direction)
        {
            case DIRECTION.LEFT:
                currentDesiredPosition = new Vector2(transform.position.x - GRID_SIZE_REFERENCE, transform.position.y);
                C_ANIMATOR.Play("LeftMove");
                break;
            case DIRECTION.RIGHT:
                currentDesiredPosition = new Vector2(transform.position.x + GRID_SIZE_REFERENCE, transform.position.y);
                C_ANIMATOR.Play("RightMove");
                break;
            case DIRECTION.UP:
                currentDesiredPosition = new Vector2(transform.position.x, transform.position.y + GRID_SIZE_REFERENCE);
                C_ANIMATOR.Play("UpMove");
                break;
            case DIRECTION.DOWN:
                currentDesiredPosition = new Vector2(transform.position.x, transform.position.y - GRID_SIZE_REFERENCE);
                C_ANIMATOR.Play("DownMove");
                break;
        }
        isAbleToMove = false;
        currentDirection = desired_direction;
    }

    void UpdateLifeIconDisplay()
    {
        if (hudLifeIcons.Length != 0)
        {
            for (int i = 0; i < hudLifeIcons.Length; i++)
            {
                if (remainingLives > i)
                {
                    hudLifeIcons[i].enabled = true;
                }
                else
                {
                    hudLifeIcons[i].enabled = false;
                }
            }
        }
    }

    // Update is called once per frame
    void Update () {
        if (isAlive) {
            if (transform.position.x % GRID_SIZE_REFERENCE == 0 && transform.position.y % GRID_SIZE_REFERENCE == 0)
            {
                isAbleToMove = true;
            }
            else
            {
                isAbleToMove = false;
            }


            if (isAbleToMove) //If the player is able to move and therefore not moving currently.
            {
                //Setting the players facing direction based on what arrow key is pressed.
                if (Input.GetKey(KeyCode.LeftArrow)) currentDirection = DIRECTION.LEFT;
                if (Input.GetKey(KeyCode.RightArrow)) currentDirection = DIRECTION.RIGHT;
                if (Input.GetKey(KeyCode.UpArrow)) currentDirection = DIRECTION.UP;
                if (Input.GetKey(KeyCode.DownArrow)) currentDirection = DIRECTION.DOWN;

                //getting player input and ensuring they are not going to or are about to walk into a wall.
                if (Input.GetKey("a") && isAbleToMove && !Physics2D.Raycast(transform.position, Vector2.left, GRID_SIZE_REFERENCE, LAYERMASK_WALL))
                {
                    Move(DIRECTION.LEFT);
                    AUDIO_MASTER.PlayOneShot(AC_WALK,1f);
                }
                else
                if (Input.GetKey("d") && isAbleToMove && !Physics2D.Raycast(transform.position, Vector2.right, GRID_SIZE_REFERENCE, LAYERMASK_WALL))
                {
                    Move(DIRECTION.RIGHT);
                    AUDIO_MASTER.PlayOneShot(AC_WALK, 1f);
                }
                else
                if (Input.GetKey("w") && isAbleToMove && !Physics2D.Raycast(transform.position, Vector2.up, GRID_SIZE_REFERENCE, LAYERMASK_WALL))
                {
                    Move(DIRECTION.UP);
                    AUDIO_MASTER.PlayOneShot(AC_WALK, 1f);
                }
                else
                if (Input.GetKey("s") && isAbleToMove && !Physics2D.Raycast(transform.position, Vector2.down, GRID_SIZE_REFERENCE, LAYERMASK_WALL))
                {
                    Move(DIRECTION.DOWN);
                    AUDIO_MASTER.PlayOneShot(AC_WALK, 1f);
                }
                else
                {
                    switch (currentDirection)
                    {
                        case DIRECTION.LEFT:
                            C_ANIMATOR.Play("LeftIdle");
                            break;
                        case DIRECTION.RIGHT:
                            C_ANIMATOR.Play("RightIdle");
                            break;
                        case DIRECTION.UP:
                            C_ANIMATOR.Play("UpIdle");
                            break;
                        case DIRECTION.DOWN:
                            C_ANIMATOR.Play("DownIdle");
                            break;
                        default:
                            C_ANIMATOR.Play("RightIdle");
                            break;
                    }
                }
            }

            transform.position = Vector2.MoveTowards(transform.position, currentDesiredPosition, MOVEMENT_SPEED * Time.deltaTime);

            if (Input.GetKeyDown("z") && BulletCountIsBelow(MAX_BULLET_COUNT) && isAbleToFire && currentAmmo.red > 0)
            {
                Fire(Bullet.COLOR.RED);
                currentAmmo.UpdateCount(-1, 0, 0);
                UpdateAmmoCounterDisplays();
            }

            if (Input.GetKeyDown("x") && BulletCountIsBelow(MAX_BULLET_COUNT) && isAbleToFire && currentAmmo.green > 0)
            {
                Fire(Bullet.COLOR.GREEN);
                currentAmmo.UpdateCount(0, -1, 0);
                UpdateAmmoCounterDisplays();
            }

            if (Input.GetKeyDown("c") && BulletCountIsBelow(MAX_BULLET_COUNT) && isAbleToFire && currentAmmo.blue > 0)
            {
                Fire(Bullet.COLOR.BLUE);
                currentAmmo.UpdateCount(0, 0, -1);
                UpdateAmmoCounterDisplays();
            }

        }

    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.GetComponent<AmmoPickup>() != null)
        {
            currentAmmo.UpdateCount(collision.gameObject.GetComponent<AmmoPickup>().RED, collision.gameObject.GetComponent<AmmoPickup>().GREEN, collision.gameObject.GetComponent<AmmoPickup>().BLUE);
            Destroy(collision.gameObject);
            UpdateAmmoCounterDisplays();
        }

        if (collision.gameObject.GetComponent<Zombie>() != null)
        {
            //KILL

            StartCoroutine("Die");
        }
    }

    IEnumerator StartGame()
    {
        isAlive = false;
        Time.timeScale = 0;
        AUDIO_MASTER.PlayOneShot(AC_GAMESTART,1f);
        ReturnToStart();
        {
            currentDirection = DIRECTION.RIGHT;
            C_ANIMATOR.Play("RightIdle");
        }
        hudTextMsgCentre.text = "HERE THEY COME..";
        yield return new WaitForSecondsRealtime(4);
        hudTextMsgCentre.text = "";
        Time.timeScale = 1;
        isAlive = true;
    }

    IEnumerator Die()
    {
        //StopAudio();
        AUDIO_MASTER.Stop();
        isAlive = false;
        Time.timeScale = 0;
        currentDesiredPosition = transform.position;
        yield return new WaitForSecondsRealtime(2);
        ZombieSpawner.WipeExistingZombies();
        PauseAllSpawners();
        AUDIO_MASTER.PlayOneShot(AC_DEATH, 1f);
        C_ANIMATOR.Play("Die");
        Time.timeScale = 1;
        yield return new WaitForSecondsRealtime(4);
        remainingLives -= 1;
        UpdateLifeIconDisplay();
        if (remainingLives > 0) //Meaning the player has not lost their last life
        {
            ReturnToStart();
            {
                currentDirection = DIRECTION.RIGHT;
                C_ANIMATOR.Play("RightIdle");
            }
            hudTextMsgCentre.text = "GET READY...";
            yield return new WaitForSecondsRealtime(1.5f);
            hudTextMsgCentre.text = "";
            isAlive = true;
            StartAllSpawners();
        }
        else
        {
            gameObject.GetComponent<SpriteRenderer>().enabled = false;
            hudTextMsgCentre.text = "GAME  OVER";
            yield return new WaitForSecondsRealtime(3f);
            hudTextMsgCentre.text = "";
            MENU_TRANSITION_COVER.SetActive(true);
            yield return new WaitForSecondsRealtime(.3f);
            ScoreCounter.WriteHighScore();
            SceneManager.LoadScene("mainmenu", LoadSceneMode.Single);

        }
    }


    void Fire(Bullet.COLOR desired_bullet_type)
    {
        isAbleToFire = false;
        Invoke("ResetFire", FIRE_COOLDOWN_TIME);
        GameObject fired_bullet = Instantiate(BULLET, transform.position,Quaternion.identity);
        switch (currentDirection)
        {
            case DIRECTION.LEFT:
                fired_bullet.transform.position = (transform.position + Vector3.left * GRID_SIZE_REFERENCE);
                fired_bullet.transform.rotation = Quaternion.Euler(0,0,180);
                break;
            case DIRECTION.RIGHT:
                fired_bullet.transform.position = (transform.position + Vector3.right * GRID_SIZE_REFERENCE);
                fired_bullet.transform.rotation = Quaternion.Euler(0, 0, 0);
                break;
            case DIRECTION.UP:
                fired_bullet.transform.position = (transform.position + Vector3.up * GRID_SIZE_REFERENCE);
                fired_bullet.transform.rotation = Quaternion.Euler(0, 0, 90);
                break;
            case DIRECTION.DOWN:
                fired_bullet.transform.position = (transform.position + Vector3.down * GRID_SIZE_REFERENCE);
                fired_bullet.transform.rotation = Quaternion.Euler(0, 0, -90);
                break;
        }
        fired_bullet.GetComponent<Bullet>().TYPE = desired_bullet_type;
        AUDIO_MASTER.PlayOneShot(AC_FIRE, 1f);
    }

    void ResetFire()
    {
        isAbleToFire = true;
    }

    bool BulletCountIsBelow(int amount)
    {
        Bullet[] current_bullets;
        current_bullets = FindObjectsOfType<Bullet>();
        if (current_bullets.Length >= amount)
        {
            return false;
        }
        else
            return true;
    }

    void UpdateAmmoCounterDisplays()
    {
        hudTextRedAmmo.text = currentAmmo.red.ToString("00");
        hudTextGreenAmmo.text = currentAmmo.green.ToString("00");
        hudTextBlueAmmo.text = currentAmmo.blue.ToString("00");
    }

}
