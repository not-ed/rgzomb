using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZombieSpawner : MonoBehaviour {

    [SerializeField]
    private Zombie.DIRECTION[] SPAWN_DIRECTIONS;

    private float minSpawnRate = 20f, maxSpawnRate = 23f;
    private float desiredMinSpawnRate = 20f, desiredMaxSpawnRate = 23f;
    private float[] SPAWN_RATE_STAGES = new float[] {20f,19.609f,19.065f,18.147f,16.6f,14.339f,11.619f,8.865f,6.536f,4.904f,3.918f,3.374f,3f};
    private int currentStageSpawn = 0;

    private float leaveSpawnRate = 3f; //how long after spawning the zombie will leave the barricade.
    private float desiredLeaveSpawnRate = 3f;
    private float[] LEAVE_SPAWN_RATE_STAGES = new float[] {3f,2.658f,2.348f,2.084f,1.884f,1.74f,1.638f,1.564f,1.508f,1.456f,1.4f,1.292f,1f};
    private int currentStageLeave = 0;

    private float hyperAwareTime = 30f;
    private float desiredHyperAwareTime = 30f;
    private float[] HYPER_AWARE_TIME_STATES = new float[] { 30f, 29.65f, 29.275f, 28.85f, 28.325f, 27.6f, 26.575f, 24.9f, 21.15f, 12.625f, 8.8f, 6.7f, 5f};
    private int currentStageHyper = 0;

    [HideInInspector]
    public static List<GameObject> existingZombies = new List<GameObject>();
    private int MAX_ZOMBIE_COUNT_HARD = 10; //how high the current max zombie count can go.
    [HideInInspector]
    public int currentMaxZombieCount = 4;

    [SerializeField]
    private GameObject ZOMBIE;

    private int GRID_SIZE_REFERENCE = 16; //The reference size of the grid in pixels which the zombie will be aligned to when moving.

    // Use this for initialization
    void Start () {
	}
	
    public void IncreaseValueStages() {
        if (currentMaxZombieCount <= MAX_ZOMBIE_COUNT_HARD)
        {
            currentMaxZombieCount += 1;
        }

        if (currentStageLeave + 1 < LEAVE_SPAWN_RATE_STAGES.Length)
        {
            currentStageLeave += 1;
            desiredLeaveSpawnRate = LEAVE_SPAWN_RATE_STAGES[currentStageLeave];
        }

        if (currentStageHyper + 1 < HYPER_AWARE_TIME_STATES.Length)
        {
            currentStageHyper += 1;
            desiredHyperAwareTime = HYPER_AWARE_TIME_STATES[currentStageHyper];
        }

        if (currentStageSpawn + 1 < SPAWN_RATE_STAGES.Length)
        {
            currentStageSpawn += 1;
            desiredMinSpawnRate = SPAWN_RATE_STAGES[currentStageSpawn];
            desiredMaxSpawnRate = desiredMinSpawnRate + 3f;
        }

    }

	// Update is called once per frame
	void Update () {

        leaveSpawnRate = Mathf.Lerp(leaveSpawnRate, desiredLeaveSpawnRate, 0.1f * Time.deltaTime);
        hyperAwareTime = Mathf.Lerp(hyperAwareTime, desiredHyperAwareTime, 0.1f * Time.deltaTime);
        minSpawnRate = Mathf.Lerp(minSpawnRate, desiredMinSpawnRate, 0.1f * Time.deltaTime);
        maxSpawnRate = Mathf.Lerp(maxSpawnRate, desiredMaxSpawnRate, 0.1f * Time.deltaTime);
    }

    //spawns a zombie and adds it to the global list.
    void Spawn()
    {
        if (existingZombies.Count != currentMaxZombieCount) {
            int color_seed = Random.Range(0, 3);
            GameObject spawned_zombie = Instantiate(ZOMBIE, transform.position, Quaternion.identity);
            switch (color_seed)
            {
                case 0:
                    spawned_zombie.GetComponent<Zombie>().SetColor(Bullet.COLOR.RED);
                    break;
                case 1:
                    spawned_zombie.GetComponent<Zombie>().SetColor(Bullet.COLOR.GREEN);
                    break;
                case 2:
                    spawned_zombie.GetComponent<Zombie>().SetColor(Bullet.COLOR.BLUE);
                    break;
            }
            existingZombies.Add(spawned_zombie);
            Zombie.DIRECTION spawn_leave_position = SPAWN_DIRECTIONS[Random.Range(0,SPAWN_DIRECTIONS.Length)];
            switch (spawn_leave_position)
            {
                case Zombie.DIRECTION.LEFT:
                    spawned_zombie.GetComponent<Zombie>().currentDesiredPosition = new Vector2(spawned_zombie.transform.position.x - GRID_SIZE_REFERENCE, spawned_zombie.transform.position.y);
                    spawned_zombie.GetComponent<Zombie>().currentDirection = Zombie.DIRECTION.LEFT;
                    break;
                case Zombie.DIRECTION.RIGHT:
                    spawned_zombie.GetComponent<Zombie>().currentDesiredPosition = new Vector2(spawned_zombie.transform.position.x + GRID_SIZE_REFERENCE, spawned_zombie.transform.position.y);
                    spawned_zombie.GetComponent<Zombie>().currentDirection = Zombie.DIRECTION.RIGHT;
                    break;
                case Zombie.DIRECTION.UP:
                    spawned_zombie.GetComponent<Zombie>().currentDesiredPosition = new Vector2(spawned_zombie.transform.position.x, spawned_zombie.transform.position.y + GRID_SIZE_REFERENCE);
                    spawned_zombie.GetComponent<Zombie>().currentDirection = Zombie.DIRECTION.UP;
                    break;
                case Zombie.DIRECTION.DOWN:
                    spawned_zombie.GetComponent<Zombie>().currentDesiredPosition = new Vector2(spawned_zombie.transform.position.x, spawned_zombie.transform.position.y - GRID_SIZE_REFERENCE);
                    spawned_zombie.GetComponent<Zombie>().currentDirection = Zombie.DIRECTION.DOWN;
                    break;
                default:
                    Destroy(spawned_zombie);
                    break;
            }
            spawned_zombie.GetComponent<Zombie>().currentMovementMode = Zombie.MOVEMENT_MODE.SPAWN;
            spawned_zombie.GetComponent<Zombie>().Invoke("ExitSpawnMode",leaveSpawnRate);
            spawned_zombie.GetComponent<Zombie>().Invoke("BecomeHyperAware", hyperAwareTime + leaveSpawnRate);
            spawned_zombie.GetComponent<Zombie>().UpdateAnimation();
        }
        Invoke("Spawn", Random.Range(minSpawnRate, maxSpawnRate));
    }

    public void PauseZombieSpawns()
    {
        CancelInvoke("Spawn");
    }

    public void StartZombieSpawns()
    {
        Invoke("Spawn", Random.Range(minSpawnRate, maxSpawnRate));
    }

    public static void WipeExistingZombies()
    {

        foreach (var zombie in existingZombies)
        {
            zombie.GetComponent<Zombie>().willGiveScoreOnDestroy = false;
            Destroy(zombie);
        }

    }
}
