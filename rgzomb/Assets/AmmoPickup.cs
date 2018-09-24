using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmmoPickup : MonoBehaviour {

    public int RED, GREEN, BLUE;
    [HideInInspector]
    public AmmoSpawner ORIGIN_SPAWNER;

    [HideInInspector]
    public bool willReinvokeOnDestroy = true; //whether to prepare the next item or not when being removed.

    [SerializeField]
    private int scoreToGive; //how many points are given when they are picked up.

    void OnDestroy()
    {
        if (ORIGIN_SPAWNER != null && willReinvokeOnDestroy)
        {
            ORIGIN_SPAWNER.PrepareNextItem(true);
        }

        ScoreCounter.currentScore += scoreToGive;
    }

}
