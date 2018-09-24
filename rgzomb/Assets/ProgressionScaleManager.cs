using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProgressionScaleManager : MonoBehaviour {

    private ZombieSpawner[] ZOMBIE_SPAWNERS;
    private AmmoSpawner[] AMMO_SPAWNERS;


    void Awake()
    {
        ZOMBIE_SPAWNERS = FindObjectsOfType<ZombieSpawner>();
        AMMO_SPAWNERS = FindObjectsOfType<AmmoSpawner>();
    }
    // Use this for initialization
    void Start () {
        StartCoroutine("ScaleValues");
	}

    IEnumerator ScaleValues()
    {
        while (true)
        {
            yield return new WaitForSeconds(30f);

            foreach (var item in ZOMBIE_SPAWNERS)
            {
                item.IncreaseValueStages();
            }

            foreach (var item in AMMO_SPAWNERS)
            {
                item.SetRateToNextStage();
            }
        }
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
