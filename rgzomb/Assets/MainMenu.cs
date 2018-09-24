using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour {

    //Constant Variables.
    [SerializeField]
    private GameObject BACKING; //The black screen backing.
    private bool gameHasStarted = false; //whether the game has started, done after button is pressed.

	// Use this for initialization
	void Start () {
        ScoreCounter.currentScore = 0;
    }
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKeyDown("z") && gameHasStarted == false)
        {
            gameHasStarted = true;
            BACKING.SetActive(true);
            Invoke("LoadToGame", .3f);
        }
	}

    void LoadToGame()
    {
        SceneManager.LoadScene("mainscene",LoadSceneMode.Single);
    }

}
