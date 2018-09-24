using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmmoSpawner : MonoBehaviour {
    [SerializeField]
    private GameObject[] ITEMS;
    private int itemNumberToSpawn;
    [SerializeField]
    private float minSpawnRate, maxSpawnRate;
    [HideInInspector]
    private float desiredMinSpawnRate = 5f, desiredMaxSpawnRate = 12.5f;
    private float[] MIN_SPAWN_RATE_STAGES = new float[] {5f,7.24f,9.2175f,10.81f,11.9825f,12.77f,13.4f,13.9975f,14.6075f,15.4825f,16.9525f,19.315f,22.5f};
    private float[] MAX_SPAWN_RATE_STAGES = new float[] {12.5f,15.625f,17.785f,19.99f,21.8625f,23.4375f,24.82f,26.0625f,27.1825f,28.18f,29.02f,29.6325f,30f};
    private int currentStage = 0;
    private GameObject spawnedItem; //the ammo pickup thats spawned.

    void Start()
    {
        SetRateToNextStage();
    }

    public void SetRateToNextStage()
    {
        if (currentStage + 1 < MIN_SPAWN_RATE_STAGES.Length && currentStage + 1 < MAX_SPAWN_RATE_STAGES.Length) {
            currentStage += 1;
            desiredMinSpawnRate = MIN_SPAWN_RATE_STAGES[currentStage];
            desiredMaxSpawnRate = MAX_SPAWN_RATE_STAGES[currentStage];
        }

    }

    void Update()
    {
        if (spawnedItem == null) Debug.Log("NO AMMO SPAWNED");

        minSpawnRate = Mathf.Lerp(minSpawnRate,desiredMinSpawnRate, 0.1f * Time.deltaTime);
        maxSpawnRate = Mathf.Lerp(maxSpawnRate, desiredMaxSpawnRate, 0.1f * Time.deltaTime);
    }

    public void PrepareNextItem(bool force_spawn)
    {
        if (spawnedItem == null)
        {
            itemNumberToSpawn = Random.Range(0, ITEMS.Length);

            Invoke("SpawnItem", Random.Range(minSpawnRate, maxSpawnRate));
        }
        else if (force_spawn)
        {
            itemNumberToSpawn = Random.Range(0, ITEMS.Length);

            Invoke("SpawnItem", Random.Range(minSpawnRate, maxSpawnRate));
        }
    }

    void SpawnItem()
    {
        spawnedItem = Instantiate(ITEMS[itemNumberToSpawn],transform.position,Quaternion.identity);
        spawnedItem.GetComponent<AmmoPickup>().ORIGIN_SPAWNER = gameObject.GetComponent<AmmoSpawner>();
    }

    public void Stop()
    {
        CancelInvoke("SpawnNextItem");
        CancelInvoke("PrepareNextItem");
        CancelInvoke("SpawnItem");
    }

    void OnApplicationQuit()
    {
        CancelInvoke("SpawnNextItem");
    }
}
