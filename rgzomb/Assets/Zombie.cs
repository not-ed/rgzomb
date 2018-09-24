using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Zombie : MonoBehaviour {

    /*Movement/Behaviour states for the zombie.
    SPAWN: Used when created using spawner, moves to desired position then goes to roam.
    ROAM: Travels maze aimlessly, no particular pattern and unpredictable.
    CHASE: Chases the player upon detection, switches back to roam upon losing sight.
    */
    public enum MOVEMENT_MODE
    {
        SPAWN, ROAM, CHASE
    }

    public enum DIRECTION
    {
        LEFT,RIGHT,UP,DOWN
    }

    //Constant Variables
    [SerializeField]
    private float MOVEMENT_SPEED; //The speed in which the zombie moves when roaming.
    [SerializeField]
    private float MOVEMENT_SPEED_HYPER; //The speed in which the zombie moves when roaming when hyper aware.
    [SerializeField]
    private float MOVEMENT_SPEED_CHASE; //The speed in which the zombie moves when chasing.
    private int GRID_SIZE_REFERENCE = 16; //The reference size of the grid in pixels which the zombie will be aligned to when moving.
    private int LAYERMASK_WALL; // The Layer Mask for the Layer named "Wall".
    [SerializeField]
    private LayerMask LAYERMASKS_ROAMING; //layers that are checked for while roaming.
    [SerializeField]
    public Bullet.COLOR COLOR; // the zombies color.
    private AudioSource AUDIO_MASTER; //The SoundManager in the game, only one audiosource should exist, which all objects will feed in to.
    [SerializeField]
    private GameObject DEATH_OBJECT; //The object displaying the death animation that the zombie drops drops.

    //component variables
    private SpriteRenderer C_SPRITE_RENDERER; //The objects "Sprite Renderer" component.
    private Animator C_ANIMATOR; //The objects "animator" component.

    private bool isHyperAware = false; //When active, will mean its easier to spot the player, searches in all directions regardless of facing dir.
    private float currentMovementSpeed; //The zombies current movement speed.
    public MOVEMENT_MODE currentMovementMode;
    public DIRECTION currentDirection = DIRECTION.RIGHT;
    [HideInInspector]
    public Vector2 currentDesiredPosition; //The destination position as to where the zombie is moving to.
    [SerializeField]
    private int lookDistanceMultiplier = 4; //how far the zombies sight raycast will reach when searching
    private bool canLeaveSpawn = false; //whether the zombie can now move after spawning, set after spawning to give a delay.
    public bool willGiveScoreOnDestroy = true; //whether score will be given when the zombie is removed from the game world, can be set to false in instances where they are removed from the game without scoring.
    [SerializeField]
    private int scoreToGive; //How much score is given when the zombie is killed.
    [SerializeField]
    private int scoreToGiveHyper; //How much score is given when the zombie is killed while hyper aware.
    [SerializeField]
    private int scoreToGiveChase; //How much score is given when the zombie is killed while in chase mode.


    //sound effects
    [SerializeField]
    private AudioClip AC_CHASE, AC_DEATH;
    private bool canPlayChaseSFX = false; //reset when the zombie reaches its chase target, prevents sound playing multiple times in a few frames.


    void InitializeComponents() //Establishes the values for component-based constants.
    {
        C_SPRITE_RENDERER = gameObject.GetComponent<SpriteRenderer>();
        C_ANIMATOR = gameObject.GetComponent<Animator>();
        AUDIO_MASTER = FindObjectOfType<AudioSource>();
    }

    void Awake()
    {
        InitializeComponents();
    }

    // Use this for initialization
    void Start () {
        //Grabs the layer mask for the "Wall" layer for use later on. Has to be done in start as the value cannot be correctly gathered using Awake.
        LAYERMASK_WALL = LayerMask.GetMask("Wall");
	}

    public void BecomeHyperAware() //makes zombie "Hyper Aware". searches for player in all directions.
    {
        isHyperAware = true;
        StartCoroutine("FlashSprite",C_SPRITE_RENDERER.color);
    }

    IEnumerator FlashSprite(Color original_color)
    {
        for (int i = 0; i < 60; i++)
        {
            C_SPRITE_RENDERER.color = Color.white;
            yield return new WaitForSeconds(1/5);
            C_SPRITE_RENDERER.color = original_color;
            yield return new WaitForSeconds(1 /5);
        }
        C_SPRITE_RENDERER.color = original_color;
    }

    public void ExitSpawnMode()
    {
        canLeaveSpawn = true;
    }

    public void SetColor(Bullet.COLOR desired_color)
    {
        COLOR = desired_color;
        switch (desired_color)
        {
            case Bullet.COLOR.RED:
                C_SPRITE_RENDERER.color = Color.red;
                break;
            case Bullet.COLOR.GREEN:
                C_SPRITE_RENDERER.color = Color.green;
                break;
            case Bullet.COLOR.BLUE:
                C_SPRITE_RENDERER.color = Color.blue;
                break;
        }
    }

    void Update()
    {

        if (currentMovementMode != MOVEMENT_MODE.CHASE) {
            if (isHyperAware) currentMovementSpeed = MOVEMENT_SPEED_HYPER; else currentMovementSpeed = MOVEMENT_SPEED;
        }
        else
        {
            currentMovementSpeed = MOVEMENT_SPEED_CHASE;
        }

        switch (currentMovementMode) //which state the zombie is in.
        {
            case MOVEMENT_MODE.SPAWN: //SPAWN MODE

                if (canLeaveSpawn)
                {
                    transform.position = Vector2.MoveTowards(transform.position, currentDesiredPosition, currentMovementSpeed * Time.deltaTime);
                }

                if (IsInPosition(currentDesiredPosition)) //meaning they are in the desired spawn position.
                {
                    currentMovementMode = MOVEMENT_MODE.ROAM;
                    canPlayChaseSFX = true;
                }

                break;


            case MOVEMENT_MODE.ROAM: //ROAM MODE

                transform.position = Vector2.MoveTowards(transform.position, currentDesiredPosition, currentMovementSpeed * Time.deltaTime);


                CheckForPlayer();

                if (IsInPosition(currentDesiredPosition) && IsAllignedToGrid() && currentMovementMode == MOVEMENT_MODE.ROAM)
                {
                    MoveRandom(Random.Range(0, 4));
                }

                break;


            case MOVEMENT_MODE.CHASE: //CHASE MODE

                transform.position = Vector2.MoveTowards(transform.position, currentDesiredPosition, currentMovementSpeed * Time.deltaTime);

                CheckForPlayer();

                if (IsInPosition(currentDesiredPosition))
                {
                    currentMovementMode = MOVEMENT_MODE.ROAM;
                    canPlayChaseSFX = true;
                }

                break;
        }
    }

    bool IsAllignedToGrid()
    {
        if (transform.position.x % GRID_SIZE_REFERENCE == 0 && transform.position.y % GRID_SIZE_REFERENCE == 0)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    bool IsAllignedToGridX()
    {
        if (transform.position.x % GRID_SIZE_REFERENCE == 0)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    bool IsAllignedToGridY()
    {
        if (transform.position.y % GRID_SIZE_REFERENCE == 0)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    bool IsInPosition(Vector2 pos)
    {
        if (transform.position.x == pos.x && transform.position.y == pos.y)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    void CheckForPlayer()
    {
        if (isHyperAware)
        {
            RaycastHit2D l_hit = Physics2D.Raycast(transform.position, Vector2.left, GRID_SIZE_REFERENCE * lookDistanceMultiplier, LAYERMASKS_ROAMING);
            RaycastHit2D r_hit = Physics2D.Raycast(transform.position, Vector2.right, GRID_SIZE_REFERENCE * lookDistanceMultiplier, LAYERMASKS_ROAMING);
            RaycastHit2D u_hit = Physics2D.Raycast(transform.position, Vector2.up, GRID_SIZE_REFERENCE * lookDistanceMultiplier, LAYERMASKS_ROAMING);
            RaycastHit2D d_hit = Physics2D.Raycast(transform.position, Vector2.down, GRID_SIZE_REFERENCE * lookDistanceMultiplier, LAYERMASKS_ROAMING);
            if (l_hit && l_hit.transform.gameObject.tag == "Player" && IsAllignedToGridY())
            {
                currentMovementMode = MOVEMENT_MODE.CHASE;
                currentDirection = DIRECTION.LEFT;
                currentDesiredPosition = new Vector2(Mathf.RoundToInt(l_hit.transform.position.x / GRID_SIZE_REFERENCE) * GRID_SIZE_REFERENCE, currentDesiredPosition.y);
                if (canPlayChaseSFX)
                {
                    AUDIO_MASTER.PlayOneShot(AC_CHASE, 1f);
                    canPlayChaseSFX = false;
                }
            }
            else if (r_hit && r_hit.transform.gameObject.tag == "Player" && IsAllignedToGridY())
            {
                currentMovementMode = MOVEMENT_MODE.CHASE;
                currentDirection = DIRECTION.RIGHT;
                currentDesiredPosition = new Vector2(Mathf.RoundToInt(r_hit.transform.position.x / GRID_SIZE_REFERENCE) * GRID_SIZE_REFERENCE, currentDesiredPosition.y);
                if (canPlayChaseSFX)
                {
                    AUDIO_MASTER.PlayOneShot(AC_CHASE, 1f);
                    canPlayChaseSFX = false;
                }
            }
            else if (u_hit && u_hit.transform.gameObject.tag == "Player" && IsAllignedToGridX())
            {
                currentMovementMode = MOVEMENT_MODE.CHASE;
                currentDirection = DIRECTION.UP;
                currentDesiredPosition = new Vector2(currentDesiredPosition.x, Mathf.RoundToInt(u_hit.transform.position.y / GRID_SIZE_REFERENCE) * GRID_SIZE_REFERENCE);
                if (canPlayChaseSFX)
                {
                    AUDIO_MASTER.PlayOneShot(AC_CHASE, 1f);
                    canPlayChaseSFX = false;
                }
            }
            else if (d_hit && d_hit.transform.gameObject.tag == "Player" && IsAllignedToGridX())
            {
                currentMovementMode = MOVEMENT_MODE.CHASE;
                currentDirection = DIRECTION.DOWN;
                currentDesiredPosition = new Vector2(currentDesiredPosition.x, Mathf.RoundToInt(d_hit.transform.position.y / GRID_SIZE_REFERENCE) * GRID_SIZE_REFERENCE);
                if (canPlayChaseSFX)
                {
                    AUDIO_MASTER.PlayOneShot(AC_CHASE, 1f);
                    canPlayChaseSFX = false;
                }
            }

            UpdateAnimation();

        }
        else
        {
            switch (currentDirection)
            {
                case DIRECTION.LEFT:
                    RaycastHit2D l_hit = Physics2D.Raycast(transform.position, Vector2.left, GRID_SIZE_REFERENCE * lookDistanceMultiplier, LAYERMASKS_ROAMING);
                    if (l_hit && l_hit.transform.gameObject.tag == "Player")
                    {
                        currentMovementMode = MOVEMENT_MODE.CHASE;
                        currentDesiredPosition = new Vector2(Mathf.RoundToInt(l_hit.transform.position.x / GRID_SIZE_REFERENCE) * GRID_SIZE_REFERENCE, transform.position.y);
                        if (canPlayChaseSFX)
                        {
                            AUDIO_MASTER.PlayOneShot(AC_CHASE, 1f);
                            canPlayChaseSFX = false;
                        }
                    }
                    break;
                case DIRECTION.RIGHT:
                    RaycastHit2D r_hit = Physics2D.Raycast(transform.position, Vector2.right, GRID_SIZE_REFERENCE * lookDistanceMultiplier, LAYERMASKS_ROAMING);
                    if (r_hit && r_hit.transform.gameObject.tag == "Player")
                    {
                        currentMovementMode = MOVEMENT_MODE.CHASE;
                        currentDesiredPosition = new Vector2(Mathf.RoundToInt(r_hit.transform.position.x / GRID_SIZE_REFERENCE) * GRID_SIZE_REFERENCE, transform.position.y);
                        if (canPlayChaseSFX)
                        {
                            AUDIO_MASTER.PlayOneShot(AC_CHASE, 1f);
                            canPlayChaseSFX = false;
                        }
                    }
                    break;
                case DIRECTION.UP:
                    RaycastHit2D u_hit = Physics2D.Raycast(transform.position, Vector2.up, GRID_SIZE_REFERENCE * lookDistanceMultiplier, LAYERMASKS_ROAMING);
                    if (u_hit && u_hit.transform.gameObject.tag == "Player")
                    {
                        currentMovementMode = MOVEMENT_MODE.CHASE;
                        currentDesiredPosition = new Vector2(transform.position.x, Mathf.RoundToInt(u_hit.transform.position.y / GRID_SIZE_REFERENCE) * GRID_SIZE_REFERENCE);
                        if (canPlayChaseSFX)
                        {
                            AUDIO_MASTER.PlayOneShot(AC_CHASE, 1f);
                            canPlayChaseSFX = false;
                        }
                    }
                    break;
                case DIRECTION.DOWN:
                    RaycastHit2D d_hit = Physics2D.Raycast(transform.position, Vector2.down, GRID_SIZE_REFERENCE * lookDistanceMultiplier, LAYERMASKS_ROAMING);
                    if (d_hit && d_hit.transform.gameObject.tag == "Player")
                    {
                        currentMovementMode = MOVEMENT_MODE.CHASE;
                        currentDesiredPosition = new Vector2(transform.position.x, Mathf.RoundToInt(d_hit.transform.position.y / GRID_SIZE_REFERENCE) * GRID_SIZE_REFERENCE);
                        if (canPlayChaseSFX)
                        {
                            AUDIO_MASTER.PlayOneShot(AC_CHASE, 1f);
                            canPlayChaseSFX = false;
                        }
                    }
                    break;
            }
        }
    }

    void MoveRandom(int move_seed)
    {
        switch (move_seed)
        {
            case 0: //right
                if (!Physics2D.Raycast(transform.position, Vector2.right, GRID_SIZE_REFERENCE, LAYERMASK_WALL))
                {
                    currentDirection = DIRECTION.RIGHT;
                    currentDesiredPosition = new Vector2(transform.position.x + GRID_SIZE_REFERENCE, transform.position.y);
                }
                break;
            case 1: //down
                if (!Physics2D.Raycast(transform.position, Vector2.down, GRID_SIZE_REFERENCE, LAYERMASK_WALL))
                {
                    currentDirection = DIRECTION.DOWN;
                    currentDesiredPosition = new Vector2(transform.position.x, transform.position.y - GRID_SIZE_REFERENCE);
                }
                break;
            case 2: //left
                if (!Physics2D.Raycast(transform.position, Vector2.left, GRID_SIZE_REFERENCE, LAYERMASK_WALL))
                {
                    currentDirection = DIRECTION.LEFT;
                    currentDesiredPosition = new Vector2(transform.position.x - GRID_SIZE_REFERENCE, transform.position.y);
                }
                break;
            case 3:
                if (!Physics2D.Raycast(transform.position, Vector2.up, GRID_SIZE_REFERENCE, LAYERMASK_WALL))
                {
                    currentDirection = DIRECTION.UP;
                    currentDesiredPosition = new Vector2(transform.position.x, transform.position.y + GRID_SIZE_REFERENCE);
                }
                break;
            default:
                break;
        }

        UpdateAnimation();

    }

    public void UpdateAnimation()
    {
        switch (currentDirection)
        {
            case DIRECTION.LEFT:
                C_ANIMATOR.Play("Left");
                break;
            case DIRECTION.RIGHT:
                C_ANIMATOR.Play("Right");
                break;
            case DIRECTION.UP:
                C_ANIMATOR.Play("Up");
                break;
            case DIRECTION.DOWN:
                C_ANIMATOR.Play("Down");
                break;
            default:
                break;
        }
    }

    public void SpawnDeathObject()
    {
        GameObject self_dead_object = Instantiate(DEATH_OBJECT, transform.position, Quaternion.identity);
        self_dead_object.GetComponent<SpriteRenderer>().color = C_SPRITE_RENDERER.color;
    }

    void OnDestroy()
    {
        ZombieSpawner.existingZombies.Remove(gameObject);
        
        if (willGiveScoreOnDestroy) {
            switch (currentMovementMode)
            {
                case MOVEMENT_MODE.SPAWN:
                    ScoreCounter.currentScore += scoreToGive;
                    break;
                case MOVEMENT_MODE.ROAM:
                    if (isHyperAware)
                    {
                        ScoreCounter.currentScore += scoreToGiveHyper;
                    }
                    else ScoreCounter.currentScore += scoreToGive;
                    break;
                case MOVEMENT_MODE.CHASE:
                    ScoreCounter.currentScore += scoreToGiveChase;
                    break;
            }
            if(AUDIO_MASTER != null) AUDIO_MASTER.PlayOneShot(AC_DEATH,1f);
        }
    }

}
